using Microsoft.Extensions.Options;
using RentCarX.Application.Helpers;
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

    NotificationStrategyOptions INotificationSender.StrategyName => NotificationStrategyOptions.Smtp;

    public async Task SendNotificationAsync(string subject, string body, CancellationToken cancellationToken, string? toEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail, nameof(toEmail));

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

            await client.SendMailAsync(mailMessage, cancellationToken);
        }
    }
}