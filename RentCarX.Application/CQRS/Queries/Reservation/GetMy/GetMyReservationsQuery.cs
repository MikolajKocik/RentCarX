using MediatR;
using RentCarX.Application.DTOs.Reservation;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetMy
{
    public record GetMyReservationsQuery() : IRequest<IEnumerable<ReservationDto>>;
}
