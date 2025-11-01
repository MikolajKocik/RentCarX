using MediatR;
using Microsoft.Extensions.Options;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.UserContext;
using RentCarX.HangfireWorker;

namespace RentCarX.Application.CQRS.Commands.Reservation.CreateReservation;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IReservationRepository _reservationRepository; 
    private readonly ICarRepository _carRepository; 
    private readonly IUserContextService _userContext;
    private readonly IEnumerable<INotificationSender> _senders;
    private readonly NotificationFeatureFlags _flags;
    private readonly IJobScheduler _jobScheduler;

    public CreateReservationCommandHandler(
        IReservationRepository reservationRepository,
        ICarRepository carRepository,
        IUserContextService userContext,
        IEnumerable<INotificationSender> senders,
        IOptions<NotificationFeatureFlags> flags,
        IJobScheduler jobScheduler
        )
    {
        _reservationRepository = reservationRepository;
        _carRepository = carRepository;
        _userContext = userContext;
        _senders = senders;
        _flags = flags.Value;
        _jobScheduler = jobScheduler;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var car = await _carRepository.GetCarByIdAsync(request.CarId, cancellationToken);
        if (car is null || !car.IsAvailable)
            throw new Exception("Car not available");

        bool overlapping = await _reservationRepository.HasOverlappingReservationAsync(request.CarId, request.StartDate, request.EndDate, cancellationToken);

        if (overlapping)
            throw new Exception("Car already reserved for this period");

        int days = (request.EndDate - request.StartDate).Days;
        decimal totalCost = days * car.PricePerDay;

        var reservation = new Domain.Models.Reservation
        {
            Id = Guid.NewGuid(),
            CarId = car.Id,
            UserId = _userContext.UserId, 
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalCost = totalCost
        };

        await _reservationRepository.Create(reservation, cancellationToken);

        car.IsAvailable = false;

        string subject = "RentCarX - your car reservation";
        string messageBody = $@"
            <html>
              <body>
                <p>Your reservation with number <strong>{reservation.Id}</strong> starts <strong>{request.StartDate}</strong> and ends <strong>{request.EndDate}</strong>.</p>
                <p>Total cost of reservation is <strong>{totalCost}</strong>.</p>
                <p>Thanks for choosing our offer. See you again!</p>
              </body>
            </html>";

        if (_flags.UseAzureNotifications)
        {
            INotificationSender azureNotificationHub = _senders.First(s => s.StrategyName.Equals(NotificationStrategyOptions.Azure));
            await azureNotificationHub.SendNotificationAsync(subject, messageBody, cancellationToken, _userContext.Email);
        }

        if (_flags.UseSmtpProtocol)
        {
            INotificationSender smtpProtocol = _senders.First(s => s.StrategyName.Equals(NotificationStrategyOptions.Smtp));
            await smtpProtocol.SendNotificationAsync(subject, messageBody, cancellationToken, _userContext.Email);
        }

        if (reservation.EndDate > DateTime.UtcNow)
        {
            _jobScheduler.SetJob(reservation);
        }

        return reservation.Id;
    } 
}
