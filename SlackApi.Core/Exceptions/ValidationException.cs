namespace SlackApi.Core.Exceptions
{
    /// <summary>
    /// An exception to be thrown when a validation error occurs.
    /// </summary>
    /// <seealso cref="BusinessException" />
    public class ValidationException : BusinessException
    {
        private const string ErrorMessage = "The model contains validation errors";

        public ValidationException() : base(ErrorMessage)
        {
        }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string field, string message) : base(!string.IsNullOrWhiteSpace(field) ? ErrorMessage : message)
        {
            if (!string.IsNullOrWhiteSpace(field))
            {
                Errors = new List<ValidationError>
                {
                    new ValidationError(field, message)
                };
            }
        }

        public ValidationException(ICollection<ValidationError> errors) : base(ErrorMessage)
        {
            Errors = errors;
        }

        public ValidationException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        public ValidationException(string message, System.Exception innerException, object extendedDetails) : base(message, innerException, extendedDetails)
        {
        }

        public ICollection<ValidationError> Errors { get; private set; }
    }

    /// <summary>
    /// Represents the validation errors of a model field
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="messages">The messages.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public ValidationError(string field, ICollection<string> messages)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            Field = field;
            Messages = messages;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public ValidationError(string field, string message)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (message == null) throw new ArgumentNullException(nameof(message));

            Field = field;
            Messages = new[] { message };
        }

        /// <summary>
        /// Gets the name of the model field with the validation error
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        public string Field { get; private set; }

        /// <summary>
        /// Gets the validation error messages for the invalid field.
        /// </summary>
        public ICollection<string> Messages { get; private set; }
    }
}
