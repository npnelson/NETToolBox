using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using NetToolBox.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.Queueing
{
    public class QueueReceiverClientTracer : BaseTracer
    {
        public QueueReceiverClientTracer(ILogger<BaseTracer> logger) : base(logger)
        {
        }

        private static readonly Action<ILogger, string, string, string, Exception> _receivedMessage = LoggerMessage.Define<string, string, string>(LogLevel.Debug, 5000, "ReceivedMessage QueueName={QueueName} MessageID={MessageID} CorrelationID={CorrelationID}");
        private static readonly Action<ILogger, string, string, string, Exception> _completedMessage = LoggerMessage.Define<string, string, string>(LogLevel.Information, 5001, "CompletedMessage QueueName={QueueName} MessageID={MessageID} CorrelationID={CorrelationID}");
        private static readonly Action<ILogger, string, string, Exception> _exceptionMessage = LoggerMessage.Define<string, string>(LogLevel.Error, 5002, "Exception while handling message QueueName={QueueName} MessageID={MessageID} The exception has been caught and will not be rethrown");
        private static readonly Action<ILogger, string, string, string, Exception> _requeueingMessage = LoggerMessage.Define<string, string, string>(LogLevel.Information, 5003, "Requeuing QueueName={QueueName} MessageID={MessageID} InitialMessageID={InitialMessageID}");
        private static readonly Action<ILogger, string, string, Exception> _expiredMessage = LoggerMessage.Define<string, string>(LogLevel.Warning, 5004, "Expiring QueueName={QueueName} MessageID={MessageID}");
        private static readonly Action<ILogger, string, string, Exception> _exceptionDeserializingMessage = LoggerMessage.Define<string, string>(LogLevel.Error, 5005, "Exception while attempting to deserialize message QueueName={QueueName} MessageID={MessageID}");
        private static readonly Action<ILogger, string, string, Exception> _exceptionRequeuingMessage = LoggerMessage.Define<string, string>(LogLevel.Error, 5006, "Exception while attempting to requeue message QueueName={QueueName} MessageID={MessageID}");
        public void ReceivedMessage(string queueName, Message msg)
        {
            _receivedMessage(_logger, queueName, msg.MessageId, msg.CorrelationId, null);
        }

        public void CompletedMessage(string queueName, Message msg)
        {
            _completedMessage(_logger, queueName, msg.MessageId, msg.CorrelationId, null);
        }

        public void ExceptionMessage(string queueName, Message msg, Exception ex)
        {
            _exceptionMessage(_logger, queueName, msg.MessageId, ex);
        }

        public void ExceptionDeserializingMessage(string queueName, Message msg, Exception ex)
        {
            _exceptionDeserializingMessage(_logger, queueName, msg.MessageId, ex);
        }

        public void RequeuingMessageExpired(string queueName, Message msg)
        {
            _expiredMessage(_logger, queueName, msg.MessageId, null);
        }
        public void RequeuingMessageCompleted(string queueName, Message msg)
        {
            _requeueingMessage(_logger, queueName, msg.MessageId, msg.UserProperties["InitialMessageId"].ToString(), null);
        }

        public void ExceptionRequeuingMessage(string queueName, Message msg, Exception ex)
        {
            _exceptionRequeuingMessage(_logger, queueName, msg.MessageId, ex);
        }
    }
}
