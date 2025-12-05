using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.Interfaces.Services.Notifications;

public interface INotificationRegister
{
    Task RegisterDeviceAsync(string pnsToken, string userEmail);
}
