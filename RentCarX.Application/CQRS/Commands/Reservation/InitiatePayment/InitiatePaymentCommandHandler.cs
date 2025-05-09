using MediatR;
using RentCarX.Domain.Interfaces.Repositories; 
using RentCarX.Domain.Exceptions; 
using RentCarX.Domain.Interfaces.Services.Stripe;

namespace RentCarX.Application.CQRS.Commands.Reservation.InitiatePayment
{
    public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, string>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IPaymentService _paymentService;

        public InitiatePaymentCommandHandler(IReservationRepository reservationRepository, IPaymentService paymentService)
        {
            _reservationRepository = reservationRepository;
            _paymentService = paymentService;
        }

        public async Task<string> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(request.ReservationId, cancellationToken);

            if (reservation == null)
            {
                throw new NotFoundException("Reservation", request.ReservationId.ToString());
            }

            var checkoutUrl = await _paymentService.CreateCheckoutSessionAsync(reservation, cancellationToken);

            return checkoutUrl; 
        }
    }
}