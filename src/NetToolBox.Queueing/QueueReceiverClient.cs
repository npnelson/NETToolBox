using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.Extensions.Logging;
using NetToolBox.Core;
using NetToolBox.Queueing.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetToolBox.Queueing
{
    public sealed class QueueReceiverClient<T> where T : class
    {
        private readonly QueueReceiverClientTracer _tracer;
        private readonly IQueueClient _queueClient;
        private readonly IMessageHandler<T> _messageHandler;
        private readonly MessageHandlerOptions _messageHandlerOptions;
        private readonly RequeuePolicy _requeuePolicy;

        public QueueReceiverClient(string connectionString, string queueName, IMessageHandler<T> messageHandler, int maxConcurrentCalls, RequeuePolicy requeuePolicy, ILogger<BaseTracer> logger) : this(new QueueClient(connectionString, queueName), messageHandler, maxConcurrentCalls, requeuePolicy, logger)
        {

        }
        internal QueueReceiverClient(IQueueClient queueClient, IMessageHandler<T> messageHandler, int maxConcurrentCalls, RequeuePolicy requeuePolicy, ILogger<BaseTracer> logger) //used for testing
        {
            _tracer = new QueueReceiverClientTracer(logger);
            _queueClient = queueClient;
            _requeuePolicy = requeuePolicy;
            _messageHandler = messageHandler;
            _messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = maxConcurrentCalls
            };
        }
        public async Task CloseAsync()
        {
            await _queueClient.CloseAsync();
        }
        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
        {
            _tracer.LogError(args.Exception, "Error in QueueReceiverClient");
            throw args.Exception;
        }

        private Message CreateRequeueMessage(Message message) //this is a method we should be able to easily test
        {
            var retval = message.Clone();
            if (!retval.UserProperties.ContainsKey("RetryCount"))
            {
                //this is our first requeue
                retval.UserProperties.Add("RetryCount", 1);
                retval.UserProperties.Add("InitialQueueTime", message.SystemProperties.EnqueuedTimeUtc);
                retval.UserProperties.Add("InitialMessageId", message.MessageId);
            }
            else
            {
                //this is a subsequent requeue
                retval.UserProperties["RetryCount"] = (int)retval.UserProperties["RetryCount"] + 1;
            }
            retval.ScheduledEnqueueTimeUtc = _requeuePolicy.CalculateNextQueueTime();
            return retval;
        }
        internal async Task ProcessMessageAsync(Message message, CancellationToken cancellationToken)
        {
            using (var scope = _tracer.BeginScope("{MessageID}", message.MessageId))
            {
                string msgBody;
                if (message.Body == null) //messages sent from legacy client
                {
                    msgBody = message.GetBody<string>();
                }
                else
                {
                    msgBody = Encoding.UTF8.GetString(message.Body);
                }
                var curlyIndex = msgBody.IndexOf('{');
                if (curlyIndex >= 0)
                {
                    msgBody = msgBody.Substring(curlyIndex); //message interop with legacy service bus client
                    var indexofbracket = msgBody.IndexOf('}');
                    if (msgBody.Length > indexofbracket + 1)
                    {
                        msgBody = msgBody.Substring(0, indexofbracket + 1);
                    }
                }
                T innerMsg = null;
                try
                {
                    innerMsg = JsonConvert.DeserializeObject<T>(msgBody);
                    _tracer.ReceivedMessage(_queueClient.Path, message);
                    var shouldHandle = await _messageHandler.HandleMessageAsync(innerMsg);
                    if (shouldHandle)
                    {
                        await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
                        _tracer.CompletedMessage(_queueClient.Path, message);
                    }
                    else
                    {
                        //test if requeue has expired
                        if (message.UserProperties.ContainsKey("InitialQueueTime") && _requeuePolicy.HasExpired((DateTime)message.UserProperties["InitialQueueTime"]))
                        {
                            _tracer.RequeuingMessageExpired(_queueClient.QueueName, message);
                            //TODO: somehow test that this expires messages only when appropriate
                            //complete this message even though a requeue has expired
                            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
                        }
                        else
                        //requeuemessage
                        {
                            var newMsg = CreateRequeueMessage(message);
                            await _queueClient.SendAsync(newMsg);
                            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
                            _tracer.RequeuingMessageCompleted(_queueClient.Path, newMsg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _tracer.ExceptionMessage(_queueClient.Path, message, ex);

                }
            }
        }

        public void RegisterHandlerAndReceiveMessages()
        {
            _queueClient.RegisterMessageHandler(ProcessMessageAsync, _messageHandlerOptions);
        }
    }
}
