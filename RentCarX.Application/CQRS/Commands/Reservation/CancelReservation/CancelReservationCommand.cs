using MediatR;

namespace RentCarX.Application.CQRS.Commands.Reservation.CancelReservation;

public sealed record CancelReservationCommand(Guid Id) : IRequest;
