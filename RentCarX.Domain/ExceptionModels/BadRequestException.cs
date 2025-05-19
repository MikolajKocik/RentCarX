namespace RentCarX.Domain.Exceptions
{
    public class BadRequestException : Exception
    {
        public string Details { get; }

        public BadRequestException(string message) : base(message)
        {
            Details = string.Empty;
        }

        public BadRequestException(string title, string message, string details)
            : base(message)
        {
            Details = details;
        }
    }
}
