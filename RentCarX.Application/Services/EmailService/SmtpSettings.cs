namespace RentCarX.Application.Services.EmailService
{
    public sealed class SmtpSettings
    {
        public required string Server { get; set; }
        public int Port { get; set; }
        public required string SenderName { get; set; }
        public required string SenderEmail { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public bool EnableSsl { get; set; }
    }
}
