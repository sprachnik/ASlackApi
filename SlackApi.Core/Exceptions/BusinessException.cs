namespace SlackApi.Core.Exceptions
{
    /// <summary>
    /// An exception to be thrown when a business rule is violated
    /// </summary>
    /// <seealso cref="Exception" />
    public class BusinessException : Exception
    {
        public object ExtendedDetails { get; set; }

        public BusinessException(string message) : base(message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
        }

        public BusinessException(string message, Exception innerException) : base(message, innerException)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (innerException == null) throw new ArgumentNullException(nameof(innerException));
        }

        public BusinessException(string message, Exception innerException, object extendedDetails) : base(message, innerException)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (innerException == null) throw new ArgumentNullException(nameof(innerException));

            ExtendedDetails = extendedDetails ?? throw new ArgumentNullException(nameof(extendedDetails));
        }
    }
}
