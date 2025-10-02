using Microsoft.Extensions.Configuration;
using RentCarX.Application.DTOs.Stripe;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.Services.Stripe;
using Stripe;
using Stripe.Checkout;

namespace RentCarX.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly StripeClient _stripeClient;
        private readonly IReservationRepository _reservationRepository;

        public PaymentService(
            IConfiguration configuration,
            StripeClient stripeClient,
            IReservationRepository reservationRepository
            )
        {
            _reservationRepository = reservationRepository;
            _configuration = configuration;
            _stripeClient = stripeClient;
        }

        public async Task<string> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, CancellationToken cancellationToken)
        {
            if (request.ReservationId == Guid.Empty)
            {
                throw new InvalidOperationException("Reservation ID is missing in the request.");
            }


            var reservation = await _reservationRepository.GetReservationByIdAsync(request.ReservationId, cancellationToken);

            if (reservation is null)
            {
                throw new KeyNotFoundException($"Reservation with ID {request.ReservationId} not found.");
            }

            if (reservation.Car is null || string.IsNullOrEmpty(reservation.Car.StripePriceId))
            {
                throw new InvalidOperationException($"Cannot create checkout session. Car or Stripe Price ID is missing for reservation {reservation.Id}. Ensure the product is synced.");
            }

            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = reservation.Car.StripePriceId,
                        Quantity = 1
                    },
                },
                Mode = "payment",

                SuccessUrl = _configuration["BaseUrl"] + "/payment/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = _configuration["BaseUrl"] + "/payment/cancel",

                Metadata = new Dictionary<string, string>
                {
                    { "reservationId", reservation.Id.ToString() }
                }
            };

            var service = new SessionService(_stripeClient);
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return session.Url;
        }
    }
}
