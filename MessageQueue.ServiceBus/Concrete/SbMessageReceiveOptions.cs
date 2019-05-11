﻿using System;
using MessageQueue.Core.Helper;
using MessageQueue.Core.Abstract;
using Microsoft.Azure.ServiceBus;
using MessageQueue.Core.Concrete;
using MessageQueue.Core.Resources;
using MessageQueue.Log.Core.Abstract;
using MessageQueue.ServiceBus.Helper;

namespace MessageQueue.ServiceBus.Concrete
{
    /// <summary>
    /// IMessageReceiveOptions implementation for ServiceBus.
    /// </summary>
    internal sealed class SbMessageReceiveOptions : IMessageReceiveOptions
    {
        #region Private Data Members
        private IQueueLogger logger;
        private string queueName;
        private string lockToken;
        private QueueClient queueClient;
        #endregion

        #region Constructors
        public SbMessageReceiveOptions(string lockToken, string queueName, bool isAcknowledgmentConfigured, ref QueueClient queueClient, ref IQueueLogger loggerObject)
        {
            #region Initialization
            this.queueName = queueName;
            this.logger = loggerObject;
            this.lockToken = lockToken;
            this.queueClient = queueClient;
            IsAcknowledgmentConfigured = isAcknowledgmentConfigured;
            #endregion
        }
        #endregion

        #region IMessageReceiveOptions Implementation
        // Properties
        public bool IsAcknowledgmentConfigured { get; }

        // Methods
        public void Acknowledge(bool ignoreError = true)
        {
            try
            {
                #region Acknowledging Message
                if (IsAcknowledgmentConfigured)
                {
                    queueClient.CompleteAsync(lockToken).Wait();
                }
                else
                {
                    throw MessageQueueCommonItems.PrepareAndLogQueueException(
                        errorCode: QueueErrorCode.AcknowledgmentIsNotConfiguredForQueue,
                        message: ErrorMessages.AcknowledgmentIsNotConfiguredForQueue,
                        innerException: null,
                        queueContext: CommonItems.ServiceBusName,
                        queueName: queueName,
                        logger: logger);
                }

                #endregion
            }
            catch (QueueException)
            {
                if (ignoreError == false)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                var queueException = MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToAcknowledgeMessage,
                    message: ErrorMessages.FailedToAcknowledgeMessage,
                    innerException: (ex is AggregateException) ? ((AggregateException)ex).Flatten() : ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: queueName,
                    logger: logger);

                if (ignoreError == false)
                {
                    throw queueException;
                }
            }
        }

        public void AbandonAcknowledgment(bool ignoreError = true)
        {
            try
            {
                #region Abandoning Acknowledgment
                if (IsAcknowledgmentConfigured)
                {
                    queueClient.AbandonAsync(lockToken).Wait();
                }
                else
                {
                    throw MessageQueueCommonItems.PrepareAndLogQueueException(
                        errorCode: QueueErrorCode.AcknowledgmentIsNotConfiguredForQueue,
                        message: ErrorMessages.AcknowledgmentIsNotConfiguredForQueue,
                        innerException: null,
                        queueContext: CommonItems.ServiceBusName,
                        queueName: queueName,
                        logger: logger);
                }
                #endregion
            }
            catch (QueueException)
            {
                if (ignoreError == false)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                var queueException = MessageQueueCommonItems.PrepareAndLogQueueException(
                    errorCode: QueueErrorCode.FailedToAbandonMessageAcknowledgment,
                    message: ErrorMessages.FailedToAbandonMessageAcknowledgment,
                    innerException: ex,
                    queueContext: CommonItems.ServiceBusName,
                    queueName: queueName,
                    logger: logger);

                if (ignoreError == false)
                {
                    throw queueException;
                }
            }
        }
        #endregion
    }
}
