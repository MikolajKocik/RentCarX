using MediatR;

namespace RentCarX.Application.CQRS.Commands.Reservation.InitiatePayment
{
    public record InitiatePaymentCommand(Guid ReservationId) : IRequest<string>; 
}
