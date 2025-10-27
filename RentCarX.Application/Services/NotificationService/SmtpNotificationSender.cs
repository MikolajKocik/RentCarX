using Microsoft.Extensions.Options;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Services.NotificationService.Settings;
using System.Net;
using System.Net.Mail;

namespace RentCarX.Application.Services.NotificationService;

public sealed class SmtpNotificationSender : INotificationSender
{
    private readonly SmtpSettings _smtpSettings;

    public SmtpNotificationSender(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendNotificationAsync(string toEmail, string subject, string body)
    {
        using (var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port))
        {
            client.EnableSsl = _smtpSettings.EnableSsl;
            client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true 
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}