using MediatR;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.UserContext;

namespace RentCarX.Application.CQRS.Commands.Reservation.CreateReservation
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
    {
        private readonly IReservationRepository _reservationRepository; 
        private readonly ICarRepository _carRepository; 
        private readonly IUserContextService _userContext;

        public CreateReservationCommandHandler(IReservationRepository reservationRepository, ICarRepository carRepository, IUserContextService userContext) // Wstrzykujemy repozytoria
        {
            _reservationRepository = reservationRepository;
            _carRepository = carRepository;
            _userContext = userContext;
        }

        public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            var car = await _carRepository.GetCarByIdAsync(request.CarId, cancellationToken);
            if (car == null || !car.IsAvailable)
                throw new Exception("Car not available");

            bool overlapping = await _reservationRepository.HasOverlappingReservationAsync(request.CarId, request.StartDate, request.EndDate, cancellationToken);

            if (overlapping)
                throw new Exception("Car already reserved for this period");

            var days = (request.EndDate - request.StartDate).Days;
            var totalCost = days * car.PricePerDay;

            var reservation = new RentCarX.Domain.Models.Reservation
            {
                Id = Guid.NewGuid(),
                CarId = car.Id,
                UserId = _userContext.UserId, 
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalCost = totalCost
            };

            await _reservationRepository.Create(reservation, cancellationToken); 

            return reservation.Id;
        }
    }
}
