using MediatR;
using RentCarX.Application.Helpers;
using RentCarX.Domain.ExceptionModels;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Commands.Reservation.DeleteReservation;

public sealed class HardDeleteReservationCommandHandler : IRequestHandler<HardDeleteReservationCommand, Unit>
{
    private readonly IReservationRepository _reservationRepository;
    public HardDeleteReservationCommandHandler(
        IReservationRepository reservationRepository)
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

        var isAlreadyDeleted = CheckReservation.IsReservationMarkedAsDeleted(reservation.Id, _reservationRepository);

        if (isAlreadyDeleted)
        {
            throw new AlreadyDeletedException("Reservation is already deleted", nameof(reservation));
        }

        if (reservation.Car is not null)
        {
            reservation.Car.IsAvailableFlag = 1;
        }

        await _reservationRepository.DeleteAsync(reservation.Id, cancellationToken);
        return Unit.Value;
    }
}
