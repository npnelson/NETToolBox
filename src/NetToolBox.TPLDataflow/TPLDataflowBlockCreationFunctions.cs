using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NetToolBox.TPLDataflow
{
    public static class TPLDataflowBlockCreationFunctions
    {
        //TODO: Handle errors
        public static IPropagatorBlock<TInput, TOutput>
     CreateUnorderedTransformBlock<TInput, TOutput>(
     Func<TInput, Task<TOutput>> func, ExecutionDataflowBlockOptions options)
        {
            var buffer = new BufferBlock<TOutput>(options);
            var action = new ActionBlock<TInput>(
                async input =>
                {
                    var output = await func(input);
                    await buffer.SendAsync(output);
                }, options);

            action.Completion.ContinueWith(
                t =>
                {
                    IDataflowBlock castedBuffer = buffer;

                    if (t.IsFaulted)
                    {
                        castedBuffer.Fault(t.Exception);
                    }
                    else if (t.IsCanceled)
                    {
                // do nothing: both blocks share options,
                // which means they also share CancellationToken
            }
                    else
                    {
                        castedBuffer.Complete();
                    }
                });

            return DataflowBlock.Encapsulate(action, buffer);
        }
    }
}
