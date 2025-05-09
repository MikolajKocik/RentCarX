namespace RentCarX.Domain.Exceptions
{
    public class ConflictException : Exception
    {
        public string Details { get; }

        public ConflictException(string message) : base(message)
        {
            Details = string.Empty;
        }

        public ConflictException(string message, string details) : base(message)
        {
            Details = details;
        }
    }
}
