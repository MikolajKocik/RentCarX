using MediatR;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Commands.Reservation.DeleteReservation
{
    public sealed class HardDeleteReservationCommandHandler : IRequestHandler<HardDeleteReservationCommand, Unit>
    {
        private readonly IReservationRepository _reservationRepository;
        public HardDeleteReservationCommandHandler(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }
        public async Task<Unit> Handle(HardDeleteReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation =  await _reservationRepository.GetReservationByIdAsync(request.Id, cancellationToken);
            if (reservation is null)
            {
                // todo logs/telemetry
                throw new NotFoundException($"Reservation with id:{request.Id} was not found", nameof(request.Id));
            }

            await _reservationRepository.DeleteAsync(reservation.Id, cancellationToken);
            return Unit.Value;
        }
    }
}
