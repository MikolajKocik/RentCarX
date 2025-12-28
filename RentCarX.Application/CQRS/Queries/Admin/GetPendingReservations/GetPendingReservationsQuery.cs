using MediatR;

namespace RentCarX.Application.CQRS.Queries.Admin.GetPendingReservations;

public sealed record GetPendingReservationsQuery : IRequest<List<Domain.Models.Reservation?>>;
