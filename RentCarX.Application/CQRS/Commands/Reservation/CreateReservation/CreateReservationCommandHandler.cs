using MediatR;
using Microsoft.Extensions.Options;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.Hangfire;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Jobs;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.UserContext;
using Microsoft.Extensions.Logging;

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
    private readonly ReservationQueueWorker _reservationQueue;
    private readonly ILogger<CreateReservationCommandHandler> _logger;

    public CreateReservationCommandHandler(
        IReservationRepository reservationRepository,
        ICarRepository carRepository,
        IUserContextService userContext,
        IEnumerable<INotificationSender> senders,
        IOptions<NotificationFeatureFlags> flags,
        IJobScheduler jobScheduler,
        IRentCarX_DbContext context,
        ReservationQueueWorker reservationQueue,
        ILogger<CreateReservationCommandHandler> logger
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
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CreateReservation requested for CarId={CarId}, Start={Start}, End={End}", request.CarId, request.StartDate, request.EndDate);

        var car = await _carRepository.GetCarByIdAsync(request.CarId, cancellationToken);
        if (car is null)
        {
            _logger.LogWarning("Car with id {CarId} was not found by repository.", request.CarId);
            throw new NotFoundException("Car not found", request.CarId.ToString());
        }

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Found car {CarId} - {Brand} {Model}", car.Id, car.Brand, car.Model);

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

            _logger.LogInformation("Reservation {ReservationId} created for car {CarId}", reservation.Id, car.Id);

            return reservation.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating reservation for car {CarId}", request.CarId);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
