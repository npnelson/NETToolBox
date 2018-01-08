using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NetToolBox.TPLDataflow
{
    public class ActionBlockPreventDuplicates<T> : ITargetBlock<T>
    {

        private readonly BufferBlock<T> StartBlock;
        private readonly ActionBlock<T> FinalBlock;
        private readonly ConcurrentDictionary<T, bool> _dictionary = new ConcurrentDictionary<T, bool>(); //we don't use the value part of the dictionary, but want to ensure we only have one entry
      


        public ActionBlockPreventDuplicates(Action<T> action, ExecutionDataflowBlockOptions dataflowBlockOptions)
        {
           
            StartBlock = new BufferBlock<T>( new ExecutionDataflowBlockOptions { BoundedCapacity = dataflowBlockOptions.MaxDegreeOfParallelism });
            FinalBlock = new ActionBlock<T>(x =>
            {
                try
                {
                    action(x);
                }
                finally
                {
                    ProcessEnd(x);
                }
              ;
            }, dataflowBlockOptions);
            CommonSetup();
        }


        public ActionBlockPreventDuplicates(Func<T, Task> action, ExecutionDataflowBlockOptions dataflowBlockOptions)
        {
          
            StartBlock = new BufferBlock<T>(new ExecutionDataflowBlockOptions { BoundedCapacity = dataflowBlockOptions.MaxDegreeOfParallelism });
            FinalBlock = new ActionBlock<T>(async x =>
            {
                try
                {
                    await action(x);
                }
                finally
                {
                    ProcessEnd(x);
                }
               ;
            }, dataflowBlockOptions);
            CommonSetup();
        }

        private void CommonSetup()
        {
            StartBlock.LinkTo(FinalBlock, item => ProcessStart(item)); //this will only forward items that aren't inflight
            StartBlock.LinkTo(DataflowBlock.NullTarget<T>()); //need a null target to deliver false messages to
        }

        private bool ProcessStart(T item)
        {
            var added = _dictionary.TryAdd(item, false);
            return added;           
        }

        private void ProcessEnd(T item)
        {
            {
                _dictionary.TryRemove(item, out var val); //we don't care if it was successful or not, tryremove will ensure the item isn't in the dictionary once completed

            }
        }

        public Task Completion => CompletionTask();

        private async Task CompletionTask()
        {
            //some interesting threading issues here, we need to do it this way to ensure pipeline completes

            await StartBlock.Completion;
            FinalBlock.Complete();
            await FinalBlock.Completion;
        }



        public void Complete()
        {
            StartBlock.Complete();

        }

        public void Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, bool consumeToAccept)
        {
            return ((ITargetBlock<T>)StartBlock).OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }
    }
    //public class ActionBlockPreventDuplicates<T>:ITargetBlock<T>
    //{
    //    private readonly ActionBlock<T> _innerBlock;
    //    private readonly IDataflowBlock _innerDataFlowBlock;
    //    private readonly ITargetBlock<T> _innerTargetBlock;
    //    private readonly ConcurrentDictionary<T,bool> _dictionary=new ConcurrentDictionary<T,bool>(); //we don't use the value part of the dictionary, but want to ensure we only have one entry
    //    public ActionBlockPreventDuplicates(Action<T> action,ExecutionDataflowBlockOptions dataflowBlockOptions)
    //    {
    //        _innerBlock = new ActionBlock<T>(GetActionWrapper(action), dataflowBlockOptions);
    //        _innerDataFlowBlock = (IDataflowBlock)_innerBlock;
    //        _innerTargetBlock = _innerBlock;
    //    }
    //    public ActionBlockPreventDuplicates(Func<T, Task> action, ExecutionDataflowBlockOptions dataflowBlockOptions)
    //    {
    //        _innerBlock = new ActionBlock<T>(GetActionWrapper(action), dataflowBlockOptions);
    //        _innerDataFlowBlock = (IDataflowBlock)_innerBlock;
    //        _innerTargetBlock = _innerBlock;
    //    }

    //    private Func<T, Task> GetActionWrapper(Func<T, Task> action)
    //    {
    //        var retval = new Func<T, Task>(x => {
    //            if (InsertItem(x))
    //            {
    //                try
    //                {
    //                    return action(x);
    //                }
    //                catch (Exception)
    //                {
    //                    throw;
    //                }
    //                finally
    //                {
    //                    RemoveItem(x);
    //                }
    //            }
    //            else
    //            {
    //                return Task.CompletedTask; //we just want to return a completed task because it is a dup
    //            }
    //        });
    //        return retval;
    //    }
    //    private Action<T> GetActionWrapper(Action<T> action)
    //    {
    //        var retval = new Action<T>(x =>
    //        {
    //            if (InsertItem(x))
    //            {
    //                try
    //                {
    //                    action(x);
    //                }
    //                catch (Exception)
    //                {
    //                    throw;
    //                }
    //                finally
    //                {
    //                    RemoveItem(x);
    //                }
    //            }
    //        });
    //        return retval;
    //    }
    //    private bool InsertItem(T item)
    //    {
    //        var added = _dictionary.TryAdd(item, false);
    //        return added;
    //    }

    //    private void RemoveItem(T item)
    //    {

    //    }



    //    public Task Completion => _innerBlock.Completion;

    //    public void Complete()
    //    {
    //        _innerBlock.Complete();
    //    }

    //     void IDataflowBlock.Fault(Exception exception)
    //    {           
    //        _innerDataFlowBlock.Fault(exception);

    //    }

    //    public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, bool consumeToAccept)
    //    {          
    //        return _innerTargetBlock.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
    //    }
    //}
}
