namespace MessageQueue.Core.Abstract
{
    /// <summary>
    /// Base class for different types of request message.
    /// </summary>
    public abstract class RequestMessage<TRequest, TResponse>
    {
        #region Public Data Members
        /// <summary>
        /// Request data.
        /// </summary>
        public TRequest RequestData { get; protected set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Responses back to the client.
        /// </summary>
        /// <param name="response">The response object</param>
        public abstract void Response(TResponse response);
        #endregion
    }
}
