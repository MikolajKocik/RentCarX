using MediatR;
using RentCarX.Application.DTOs.Reservation;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetById
{
    public sealed record GetReservationByIdQuery(Guid Id) : IRequest<ReservationDto>;
}
