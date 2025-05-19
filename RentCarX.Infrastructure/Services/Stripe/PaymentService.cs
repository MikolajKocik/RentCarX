using Microsoft.Extensions.Configuration;
using RentCarX.Domain.Models;
using Stripe;
using Stripe.Checkout;
using RentCarX.Domain.Interfaces.Services.Stripe;

namespace RentCarX.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;

        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;

            string? stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_API_KEY");

            if (string.IsNullOrEmpty(stripeSecretKey))
            {
                throw new InvalidOperationException("Stripe Secret Key environment variable 'STRIPE_API_KEY' is not set.");
            }

            StripeConfiguration.ApiKey = stripeSecretKey;
        }

        public async Task<string> CreateCheckoutSessionAsync(Reservation reservation, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(reservation.Car.StripePriceId))
            {
                throw new InvalidOperationException("This car has no Stripe Price ID assigned. Please sync it first.");
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

            var service = new SessionService();
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return session.Url;
        }
    }
}
