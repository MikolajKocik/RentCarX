using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Options;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Services.NotificationService.Settings;
using RentCarX.Domain.Interfaces.UserContext;

namespace RentCarX.Application.Services.NotificationService;

public sealed class AzureNotificationSender : INotificationSender
{
    private readonly NotificationHubClient _hubClient;
    private readonly IUserContextService _userContextService;
    public AzureNotificationSender(
        IOptions<NotificationHub> settings,
        IUserContextService userContextService
        )
    {
        _hubClient = NotificationHubClient.CreateClientFromConnectionString(
            settings.Value.ConnectionString,
            settings.Value.HubName);

        _userContextService = userContextService;
    }

    NotificationStrategyOptions INotificationSender.StrategyName => NotificationStrategyOptions.Azure;

    public async Task SendNotificationAsync(string subject, string messageBody, CancellationToken cancellationToken, string? recipientTag = null)
    {
        if (recipientTag is null)
        {
            recipientTag = _userContextService.Email 
                ?? throw new ArgumentNullException("No user's email found");
        }

        // for safety
        var hashedTag = TagHasher.UseSHA256(recipientTag);

        var notification = new Dictionary<string, string>
        {
            { "title", subject },
            { "message", messageBody }
        };

        await _hubClient.SendTemplateNotificationAsync(notification, hashedTag, cancellationToken);
    }
}
