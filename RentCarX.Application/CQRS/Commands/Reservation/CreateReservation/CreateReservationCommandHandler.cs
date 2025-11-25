using MediatR;
using Microsoft.Extensions.Options;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.UserContext;
using RentCarX.HangfireWorker;
using System.Collections.Concurrent;

namespace RentCarX.Application.CQRS.Commands.Reservation.CreateReservation;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICarRepository _carRepository;
    private readonly IUserContextService _userContext;
    private readonly IEnumerable<INotificationSender> _senders;
    private readonly NotificationFeatureFlags _flags;
    private readonly IJobScheduler _jobScheduler;
    private readonly IRentCarX_DbContext _context;
    private readonly ConcurrentQueue<Guid> _reservationQueue;

    public CreateReservationCommandHandler(
        IReservationRepository reservationRepository,
        ICarRepository carRepository,
        IUserContextService userContext,
        IEnumerable<INotificationSender> senders,
        IOptions<NotificationFeatureFlags> flags,
        IJobScheduler jobScheduler,
        IRentCarX_DbContext context,
        ConcurrentQueue<Guid> reservationQueue
        )
    {
        _reservationRepository = reservationRepository;
        _carRepository = carRepository;
        _userContext = userContext;
        _senders = senders;
        _flags = flags.Value;
        _jobScheduler = jobScheduler;
        _context = context;
        _reservationQueue = reservationQueue;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            var car = await _carRepository.GetCarByIdAsync(request.CarId, cancellationToken);

            if (car is null)
                throw new NotFoundException("Car not found", nameof(car.Id));

            if (!CheckReservation.TryReserveCar(car))
                throw new BadRequestException("Car not available");

            _reservationQueue.Enqueue(car.Id);

            bool overlapping = await _reservationRepository.HasOverlappingReservationAsync(request.CarId, request.StartDate, request.EndDate, cancellationToken);

            if (overlapping)
                throw new ConflictException("Car already reserved for this period");

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

            string subject = "RentCarX - your car reservation";
            string messageBody = $@"
            <html>
              <body>
                <p>Your reservation with number <strong>{reservation.Id}</strong> starts <strong>{request.StartDate}</strong> and ends <strong>{request.EndDate}</strong>.</p>
                <p>Total cost of reservation is <strong>{totalCost}</strong>.</p>
                <p>Thanks for choosing our offer. See you again!</p>
              </body>
            </html>";

            var tasks = new List<Task>();

            if (_flags.UseAzureNotifications)
            {
                INotificationSender azureNotificationHub = _senders.First(s => s.StrategyName.Equals(NotificationStrategyOptions.Azure));
                tasks.Add(azureNotificationHub.SendNotificationAsync(subject, messageBody, cancellationToken, _userContext.Email));
            }

            if (_flags.UseSmtpProtocol)
            {
                INotificationSender smtpProtocol = _senders.First(s => s.StrategyName.Equals(NotificationStrategyOptions.Smtp));
                tasks.Add(smtpProtocol.SendNotificationAsync(subject, messageBody, cancellationToken, _userContext.Email));
            }

            await Task.WhenAll(tasks);

            if (reservation.EndDate > DateTime.UtcNow)
            {
                _jobScheduler.SetJob(reservation);
            }

            await transaction.CommitAsync(cancellationToken);

            return reservation.Id;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
