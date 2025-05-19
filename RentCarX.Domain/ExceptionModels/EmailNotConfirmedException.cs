namespace RentCarX.Domain.Exceptions
{
    public class EmailNotConfirmedException : Exception
    {
        public string ErrorCode { get; }

        public EmailNotConfirmedException(string message) : base(message)
        {
            ErrorCode = "EmailNotConfirmed"; 
        }

        public EmailNotConfirmedException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "EmailNotConfirmed";
        }
    }
}
