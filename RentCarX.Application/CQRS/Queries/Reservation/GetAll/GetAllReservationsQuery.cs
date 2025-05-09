using MediatR;
using RentCarX.Application.DTOs.Reservation;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetAll
{
    public record GetAllReservationsQuery : IRequest<List<ReservationDto>>;

}
