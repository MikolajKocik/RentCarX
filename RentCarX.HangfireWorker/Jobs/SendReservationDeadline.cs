using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;
using RentCarX.Domain.Models.Enums;
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
        DateTime reminder = now.AddMinutes(30);

        if (_flags.Value.UseAzureNotifications)
        {
            INotificationSender? azure = _senders.FirstOrDefault(r => r.StrategyName == NotificationStrategyOptions.Azure);
            if (azure is not null)
            {
                await SetNotificationAsync(reminder, azure, cancellationToken);
            }
        }

        if (_flags.Value.UseSmtpProtocol)
        {
            INotificationSender? smtp = _senders.FirstOrDefault(s => s.StrategyName == NotificationStrategyOptions.Smtp);
            if (smtp is not null)
            {
                await SetNotificationAsync(reminder, smtp, cancellationToken);
            }
        }
    }

    private async Task SetNotificationAsync(DateTime reminder, INotificationSender notification, CancellationToken cancellationToken)
    {
        DateTime from = reminder.AddMinutes(-1);
        DateTime to = reminder.AddMinutes(1);

        List<Reservation> reservations = await _reservationRepository.GetAll()
            .Include(r => r.User)
            .Where(r => r.EndDate >= from &&
                        r.EndDate <= to &&
                        r.Status != ReservationStatus.Cancelled &&
                        r.IsPaid &&
                        !r.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!reservations.Any())
        {
            _logger.LogInformation("No reservations requiring pickup reminders at {Time}", reminder);
            return;
        }

        foreach (var reservation in reservations)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(reservation.User?.Email))
                {
                    _logger.LogWarning("Reservation {ReservationId} has no valid email for user {UserId}",
                        reservation.Id, reservation.UserId);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(reservation.User?.Email)) continue;

                if (reservation.Status == ReservationStatus.Cancelled)
                    continue;

                if (!reservation.IsPaid)
                    continue;

                string carName = $"{reservation.Car.Brand} {reservation.Car.Model}";
                string subject = "Reservation reminder";

                string messageBody = $@"
                    <html>
                        <body>
                            <h2>Reservation Reminder</h2>
                            <p>Hello,</p>
                            <p>Your reservation for <strong>{carName}</strong> ends in approximately 30 minutes.</p>
                            <p><strong>Return deadline:</strong> {reservation.EndDate:yyyy-MM-dd HH:mm} UTC</p>
                            <p>Please ensure you return the car on time to avoid additional charges.</p>
                            <br/>
                            <p>Thank you for using RentCarX!</p>
                        </body>
                    </html>";

                await notification.SendNotificationAsync(subject, messageBody, cancellationToken, reservation.User.Email);
                _logger.LogInformation("Reminder sent for reservation {ReservationId} to {Email}",
                       reservation.Id, reservation.User.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending pickup reminder for reservation {ReservationId}",
                    reservation.Id);
            }
        }
    }
}
