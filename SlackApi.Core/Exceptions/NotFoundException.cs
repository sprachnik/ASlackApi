namespace SlackApi.Core.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
        }
    }
}
