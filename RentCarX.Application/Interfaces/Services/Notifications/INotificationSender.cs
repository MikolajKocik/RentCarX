namespace RentCarX.Application.Interfaces.Services.NotificationStrategy;

public interface INotificationSender
{
    Task SendNotificationAsync(string recipientEmail, string subject, string messageBody);
}
