using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace NetToolBox.TPLDataflow
{
    public class ActionBlockPreventDuplicates<T>:ITargetBlock<T>
    {
        private readonly ActionBlock<T> _innerBlock;
        private readonly IDataflowBlock _innerDataFlowBlock;
        private readonly ITargetBlock<T> _innerTargetBlock;
        private readonly ConcurrentDictionary<T,bool> _dictionary=new ConcurrentDictionary<T,bool>(); //we don't use the value part of the dictionary, but want to ensure we only have one entry
        public ActionBlockPreventDuplicates(Action<T> action,ExecutionDataflowBlockOptions dataflowBlockOptions)
        {
            _innerBlock = new ActionBlock<T>(GetActionWrapper(action), dataflowBlockOptions);
            _innerDataFlowBlock = (IDataflowBlock)_innerBlock;
            _innerTargetBlock = _innerBlock;
        }
        public ActionBlockPreventDuplicates(Func<T, Task> action, ExecutionDataflowBlockOptions dataflowBlockOptions)
        {
            _innerBlock = new ActionBlock<T>(GetActionWrapper(action), dataflowBlockOptions);
            _innerDataFlowBlock = (IDataflowBlock)_innerBlock;
            _innerTargetBlock = _innerBlock;
        }

        private Func<T, Task> GetActionWrapper(Func<T, Task> action)
        {
            var retval = new Func<T, Task>(x => {
                if (InsertItem(x))
                {
                    try
                    {
                        return action(x);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        RemoveItem(x);
                    }
                }
                else
                {
                    return Task.CompletedTask; //we just want to return a completed task because it is a dup
                }
            });
            return retval;
        }
        private Action<T> GetActionWrapper(Action<T> action)
        {
            var retval = new Action<T>(x =>
            {
                if (InsertItem(x))
                {
                    try
                    {
                        action(x);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        RemoveItem(x);
                    }
                }
            });
            return retval;
        }
        private bool InsertItem(T item)
        {
            var added = _dictionary.TryAdd(item, false);
            return added;
        }

        private void RemoveItem(T item)
        {

        }

       

        public Task Completion => _innerBlock.Completion;

        public void Complete()
        {
            _innerBlock.Complete();
        }

         void IDataflowBlock.Fault(Exception exception)
        {           
            _innerDataFlowBlock.Fault(exception);
          
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, bool consumeToAccept)
        {          
            return _innerTargetBlock.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }
    }
}
