using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetToolBox.Core;
using NetToolBox.Queueing;
using NetToolBox.Queueing.Abstractions;
using NetToolBox.TestHelpers.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NetToolBox.Tests
{
    public class QueueReceiverClientTests
    {
        [Fact]
        public async Task HandledTrueCompletes()
        {
            var fixture = new QueueReceiverClientTestFixture();
            fixture.MockMessageHandler.Setup(x => x.HandleMessageAsync(It.IsAny<TestMessage>())).ReturnsAsync(true);

            await fixture.ProcessMessageAsyncTestAsync();

            fixture.MockQueueClient.Verify(x => x.CompleteAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task HandledFalseRequeuesIfNotExpired()
        {
            //since we aren't advancing the clock, the message shouldn't expire
            var fixture = new QueueReceiverClientTestFixture();
            fixture.MockMessageHandler.Setup(x => x.HandleMessageAsync(It.IsAny<TestMessage>())).ReturnsAsync(false);

            await fixture.ProcessMessageAsyncTestAsync();

            fixture.MockQueueClient.Verify(x => x.CompleteAsync(It.IsAny<string>()), Times.Once);
            fixture.MockQueueClient.Verify(x => x.SendAsync(It.IsAny<Message>()), Times.Once);
        }
        [Fact]
        public async Task HandledFalseDoesntRequeueIfExpired()
        {
            var fixture = new QueueReceiverClientTestFixture();
            fixture.MockMessageHandler.Setup(x => x.HandleMessageAsync(It.IsAny<TestMessage>())).ReturnsAsync(false);

            await fixture.ProcessMessageAsyncTestAsync(); //we have to requeue it at least once to get the properties in there like initialqueuetime
            fixture.TestDateTimeProvider.AdvanceDays(2); //don't really care about border cases on the expire, the fixture expiration policy is one day later
            await fixture.ProcessMessageAsyncTestAsync();
            fixture.MockQueueClient.Verify(x => x.CompleteAsync(It.IsAny<string>()), Times.Exactly(2));  //should complete 2 messages
            fixture.MockQueueClient.Verify(x => x.SendAsync(It.IsAny<Message>()), Times.Once); //should only send once due to first requeue
        }
    }
    public class TestMessage
    {
        public string TestProperty { get; set; }
    }
    public class QueueReceiverClientTestFixture
    {
        public Mock<IQueueClient> MockQueueClient = new Mock<IQueueClient>();
        public TestDateTimeProvider TestDateTimeProvider = new TestDateTimeProvider();
        public QueueReceiverClient<TestMessage> QueueReceiverClient;
        public Mock<IMessageHandler<TestMessage>> MockMessageHandler = new Mock<IMessageHandler<TestMessage>>();
        public Message LastSentMessage;
        public QueueReceiverClientTestFixture()
        {
            MockQueueClient.SetupGet(x => x.Path).Returns("TestQueue");
            MockQueueClient.Setup(x => x.SendAsync(It.IsAny<Message>())).Callback<Message>(x => LastSentMessage = x).Returns(Task.CompletedTask);
            QueueReceiverClient = new QueueReceiverClient<TestMessage>(MockQueueClient.Object, MockMessageHandler.Object, 1, new MinuteRequeuePolicy(TestDateTimeProvider), NullLogger<BaseTracer>.Instance);
        }

        public async Task ProcessMessageAsyncTestAsync()
        {
            Message msg;
            //for our tests, if lastSentMessage is sent, we want to send that instead of our new message
            //however, this might not be desirable behavior for future tests, so be careful
            if (LastSentMessage is null)
            {
                msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new TestMessage())))
                {
                    MessageId = Guid.NewGuid().ToString()
                };
            }
            else
            {
                msg = LastSentMessage;
            }

            //have to do some dancing to set internal property on message so that it thinks it is received
            Type spType = msg.SystemProperties.GetType();
            PropertyInfo seqProp = spType.GetProperty("SequenceNumber");
            seqProp.SetValue(msg.SystemProperties, (long)1);
            await QueueReceiverClient.ProcessMessageAsync(msg, new CancellationTokenSource().Token);
        }
    }
}
