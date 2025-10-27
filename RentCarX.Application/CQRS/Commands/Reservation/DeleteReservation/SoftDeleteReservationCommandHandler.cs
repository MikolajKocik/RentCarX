using MediatR;
using RentCarX.Application.Helpers;
using RentCarX.Domain.ExceptionModels;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Commands.Reservation.DeleteReservation
{
    public sealed class SoftDeleteReservationCommandHandler : IRequestHandler<SoftDeleteReservationCommand, Unit>
    {
        private readonly IReservationRepository _reservationRepository;

        public SoftDeleteReservationCommandHandler(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<Unit> Handle(SoftDeleteReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(request.Id, cancellationToken);
            if (reservation is null)
            {
                throw new NotFoundException($"Reservation with provided id:{request.Id} was not found", nameof(request.Id));
            }

            var isAlreadyDeleted = CheckReservation.IsReservationMarkedAsDeleted(reservation.Id, _reservationRepository);

            if (isAlreadyDeleted)
            {
                throw new AlreadyDeletedException("Reservation is already deleted", nameof(reservation));
            }

            reservation.IsDeleted = true;
            await _reservationRepository.SaveToDatabase(cancellationToken);
            return Unit.Value;
        }
    }
}
