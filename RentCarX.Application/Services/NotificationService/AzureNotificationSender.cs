using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Options;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;

namespace RentCarX.Application.Services.NotificationService;

public sealed class AzureNotificationSender : INotificationSender
{
    private readonly NotificationHubClient _hubClient;
    public AzureNotificationSender(IOptions<Settings.NotificationHubSettings> settings)
    {
        _hubClient = NotificationHubClient.CreateClientFromConnectionString(
            settings.Value.ConnectionString,
            settings.Value.HubName);
    }

    public async Task SendNotificationAsync(string recipientTag, string subject, string messageBody)
    {
        var notification = new Dictionary<string, string>
        {
            { "title", subject },
            { "message", messageBody }
        };

        await _hubClient.SendTemplateNotificationAsync(notification, recipientTag);
    }
}
