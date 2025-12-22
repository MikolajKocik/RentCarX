using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Options;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.Notifications;
using System.Security.Cryptography;
using System.Text;

namespace RentCarX.Application.Services.NotificationService;

public sealed class NotificationRegistrationService : INotificationRegister
{
    private readonly NotificationHubClient _hubClient;

    public NotificationRegistrationService(IOptions<Settings.NotificationHub> settings)
    {
        // the same client, who is sending the messages
        _hubClient = NotificationHubClient.CreateClientFromConnectionString(
            settings.Value.ConnectionString,
            settings.Value.HubName);
    }

    public async Task RegisterDeviceAsync(string pnsToken, string userEmail)
    {
        string installationId = GetDeterministicId(pnsToken);

        var hashedTag = TagHasher.UseSHA256(userEmail);

        var installation = new Installation
        {
            InstallationId = installationId,
            PushChannel = pnsToken, // firebase token
            Platform = NotificationPlatform.FcmV1, // FcmV1 -> Web Push
            Tags = new List<string> { hashedTag } 
        };

        // web template
        installation.Templates.Add("webTemplate", new InstallationTemplate
        {
            Body = "{\"notification\":{\"title\":\"$(title)\",\"body\":\"$(message)\"}}",
            Headers = new Dictionary<string, string> { { "apns-priority", "10" } } 
        });

        // save to azure
        await _hubClient.CreateOrUpdateInstallationAsync(installation);
    }

    private static string GetDeterministicId(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input));

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
