using RentCarX.Application.Helpers;

namespace RentCarX.Application.Interfaces.Services.NotificationStrategy;

public interface INotificationSender
{
    NotificationStrategyOptions StrategyName { get; }
    Task SendNotificationAsync(string subject, string messageBody, CancellationToken cancellationToken, string? recipientTag = null);
}
