using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.Services.NotificationService.Flags;

public sealed class NotificationFeatureFlags
{
    public bool UseAzureNotifications { get; set; }
    public bool UseSmtpProtocol { get; set; }
}
