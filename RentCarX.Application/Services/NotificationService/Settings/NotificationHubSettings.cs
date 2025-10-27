using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.Services.NotificationService.Settings;

public sealed class NotificationHubSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string HubName { get; set; } = string.Empty;
}
