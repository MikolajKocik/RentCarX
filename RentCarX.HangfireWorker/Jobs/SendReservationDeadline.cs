using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;
using RentCarX.HangfireWorker.Jobs.Abstractions;

namespace RentCarX.HangfireWorker.Jobs;

public sealed class SendReservationDeadline : JobPlanner
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IEnumerable<INotificationSender> _senders;
    private readonly IOptions<NotificationFeatureFlags> _flags;

    public SendReservationDeadline(
        IReservationRepository reservationRepository,
        IEnumerable<INotificationSender> sender,
        IOptions<NotificationFeatureFlags> flags,
        ILogger<UpdateCarAvailabilityJob> logger) : base(logger)
    {
        _reservationRepository = reservationRepository;
        _senders = sender;
        _flags = flags;
    }

    public override async Task PerformJobAsync(CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        DateTime targetTime = now.AddMinutes(30);

        if (_flags.Value.UseAzureNotifications)
        {
            INotificationSender? azure = _senders.FirstOrDefault(r => r.StrategyName == NotificationStrategyOptions.Azure);
            if (azure is not null)
            {
                await SetNotificationAsync(targetTime, azure, cancellationToken);
            }        
        }

        if (_flags.Value.UseSmtpProtocol)
        {
            INotificationSender? smtp = _senders.FirstOrDefault(s => s.StrategyName == NotificationStrategyOptions.Smtp);
            if (smtp is not null)
            {
                await SetNotificationAsync(targetTime, smtp, cancellationToken);
            }        
        }
    }

    private async Task SetNotificationAsync(DateTime targetTime, INotificationSender notification, CancellationToken cancellationToken)
    {
        DateTime from = targetTime.AddMinutes(-1);
        DateTime to = targetTime.AddMinutes(1);

        List<Reservation> reservations = await _reservationRepository.GetAll()
         .Include(r => r.User)
         .Where(r => r.EndDate >= from && r.EndDate <= to)
         .ToListAsync(cancellationToken);

        foreach (var reservation in reservations)
        {
            if (string.IsNullOrWhiteSpace(reservation.User?.Email)) continue;

            string subject = "Reservation reminder";

            string messageBody = @$"
                    <html>
                        <body>
                            <p> Your reservation ends in 30 minutes.</p>
                            <p> Please remember to return your car on time.</p>
                            <p> This message is auto generated.</p>
                        </body>
                    </html>";


            await notification.SendNotificationAsync(subject, messageBody, cancellationToken, reservation.User.Email);
        }
    }
}
