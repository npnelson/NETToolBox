using Microsoft.Extensions.Logging;
using NetToolBox.Core;
using NetToolBox.Queueing;
using NetToolBox.Queueing.Abstractions;
using NetToolBox.TPLDataflow;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NetToolBox.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            TestQueue();
            Console.ReadLine();
        }

        public static void TestQueue()
        {
            var lf = new LoggerFactory();
            lf.AddConsole();
            var requeuePolicy = new MinuteRequeuePolicy(new DateTimeProvider());
            var handler = new MessageHandler();
            var receiver = new QueueReceiverClient<TableKey>("<insert servicebus connection string>", "testresubmitqueue", handler, 1, requeuePolicy, lf.CreateLogger<NetToolBox.Core.BaseTracer>());
            receiver.RegisterHandlerAndReceiveMessages();
        }

        public static void TestTPL()
        {
            var opts = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5, BoundedCapacity = 5 };
            var block1 = new UnorderedTransformBlock<int, int>(async x => {
                Console.WriteLine(x.ToString() + "T1");
                await Task.Delay(TimeSpan.FromSeconds(x));
                return x;
            }, opts);


            var block2 = new ActionBlock<int>(x => Console.WriteLine(x), opts);
            block1.LinkTo(block2);
            //  Console.WriteLine($"Input Count={block2.InputCount}");
            block1.SendAsync(10).GetAwaiter().GetResult();
            //  Console.WriteLine($"Input Count={block2.InputCount}");
            block1.SendAsync(5);
            //  Console.WriteLine($"Input Count={block2.InputCount}");
            block1.SendAsync(1).GetAwaiter().GetResult();
            block1.SendAsync(10).GetAwaiter().GetResult();
            block1.SendAsync(9).GetAwaiter().GetResult();
            block1.SendAsync(8).GetAwaiter().GetResult();
            block1.SendAsync(7).GetAwaiter().GetResult();
            //  Console.WriteLine($"Input Count={block2.InputCount}");
            Console.ReadLine();
        }
    }

    public class MessageHandler : IMessageHandler<TableKey>
    {
        public async Task<bool> HandleMessageAsync(TableKey message)
        {
            return false;
        }
    }
}
