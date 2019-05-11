using System;
using System.Threading;
using System.Threading.Tasks;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Concrete;
using Microsoft.Azure.ServiceBus;
using System.Collections.Generic;
using MessageQueue.Core.Abstract;
using MessageQueue.Core.Resources;
using MessageQueue.ServiceBus.Helper;
using MessageQueue.Log.Core.Abstract;
using Microsoft.Azure.ServiceBus.Core;
using MessageQueue.ServiceBus.Abstract;
using MessageQueue.Core.Abstract.Inbound;

namespace MessageQueue.ServiceBus.Concrete.Inbound
{
    /// <summary>
    /// ServiceBus based implementation of IInboundFaFMq.
    /// </summary>
    internal sealed class SbInboundFaF<TMessage> : BaseServiceBus, IInboundFaFMq<TMessage>
    {
        #region Private Data Members
        private MessageReceiver messageReceiver;
        private volatile bool isReceivingMessages;
        #endregion

        #region Constructors
        public SbInboundFaF(Dictionary<string, string> configuration, IQueueLogger loggerObject)
        {
            try
            {
                #region Initialization
                base.Initialize(configuration, true, loggerObject);

                // Setting other fields.
                Address = sbConfiguration.Address;
                #endregion
            }
            catch (Exception ex) when (!(ex is QueueException))
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToInitializeMessageQueue,
                    message: ErrorMessages.FailedToInitializeMessageQueue,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }
        #endregion

        #region IInboundFaFMq Implementation
        public string Address { get; }

        public event Action<TMessage, IMessageReceiveOptions> OnMessageReady;
        public event Func<TMessage, IMessageReceiveOptions, Task> OnMessageReadyAsync;

        public async Task<bool> HasMessage()
        {
            try
            {
                var messageReceiverToPeek = new MessageReceiver(sbConfiguration.ConnectionString, ReceiveMode.PeekLock);

                // Peeking
                var result = await messageReceiverToPeek.PeekAsync() != null;

                // Closing connection.
                await messageReceiverToPeek.CloseAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToCheckQueueHasMessage,
                    message: ErrorMessages.FailedToCheckQueueHasMessage,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }

        public void StartReceivingMessage()
        {
            try
            {
                lock (queueClient)
                {
                    if (!isReceivingMessages)
                    {
                        // Creating message receiver.
                        var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                        {
                            MaxConcurrentCalls = sbConfiguration.MaxConcurrentReceiveCallback,
                            AutoComplete = false,
                        };

                        // Registering event.
                        queueClient.RegisterMessageHandler(ReceiveReadyAsync, messageHandlerOptions);

                        // Updating flag.
                        isReceivingMessages = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToStartReceivingMessage,
                    message: ErrorMessages.FailedToStartReceivingMessage,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }

        public void StopReceivingMessage()
        {
            try
            {
                lock (queueClient)
                {
                    if (isReceivingMessages)
                    {
                        messageReceiver.CloseAsync().Wait();

                        // Updating flag.
                        isReceivingMessages = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToStopReceivingMessage,
                    message: ErrorMessages.FailedToStopReceivingMessage,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            #region Cleanup
            if (disposing)
            {
                queueClient.CloseAsync().Wait();
            }
            #endregion
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Helper event handler.
        /// </summary>
        private async Task ReceiveReadyAsync(Message brokeredMessage, CancellationToken token)
        {
            try
            {
                if (brokeredMessage != null)
                {
                    // Converting from Json.
                    var convertedMessage = MessageQueueCommonItems.DeserializeFromJsonBytes<TMessage>(brokeredMessage.Body);
                    var lockToken = string.Empty;

                    // Getting lock token if acknowledgment is configured.
                    if (sbConfiguration.Acknowledgment)
                    {
                        lockToken = brokeredMessage.SystemProperties.LockToken;
                    }

                    var messageReceiveOptions = new SbMessageReceiveOptions(lockToken, sbConfiguration.QueueName, sbConfiguration.Acknowledgment, ref queueClient, ref logger);

                    // Calling handler (async is preferred over sync).
                    if (OnMessageReadyAsync != null)
                    {
                        await OnMessageReadyAsync.Invoke(convertedMessage, messageReceiveOptions);
                    }
                    else
                    {
                        OnMessageReady?.Invoke(convertedMessage, messageReceiveOptions);
                    }
                }
            }
            catch (QueueException queueException)
            {
                #region Logging - Error
                logger.Fatal(queueException, queueException.Message);
                #endregion
            }
            catch (Exception ex)
            {
                MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToReceiveMessage,
                    message: ErrorMessages.FailedToReceiveMessage,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: sbConfiguration.QueueName,
                    address: sbConfiguration.Address,
                    logger: logger);
            }
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToReceiveMessage,
                    message: ErrorMessages.FailedToReceiveMessage,
                    innerException: exceptionReceivedEventArgs.Exception,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: context.EntityPath,
                    address: context.Endpoint,
                    logger: null);

            return Task.CompletedTask;
        }
        #endregion
    }
}
