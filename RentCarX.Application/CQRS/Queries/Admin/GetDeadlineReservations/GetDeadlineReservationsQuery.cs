using MediatR;
using RentCarX.Application.DTOs.Reservation;

namespace RentCarX.Application.CQRS.Queries.Admin.GetDeadlineReservations;

public sealed record class GetDeadlineReservationsQuery() : IRequest<List<ReservationDeadlineDto>>;
