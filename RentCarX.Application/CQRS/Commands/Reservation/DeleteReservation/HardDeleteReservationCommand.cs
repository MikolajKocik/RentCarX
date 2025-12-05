using MediatR;

namespace RentCarX.Application.CQRS.Commands.Reservation.DeleteReservation
{
    public sealed record HardDeleteReservationCommand(Guid Id) : IRequest<Unit>;
}
