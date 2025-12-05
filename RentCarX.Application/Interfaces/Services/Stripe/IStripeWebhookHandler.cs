using Stripe;

namespace RentCarX.Domain.Interfaces.Services.Stripe;

public interface IStripeWebhookHandler
{
    Task HandleEventAsync(Event stripeEvent, CancellationToken cancellationToken = default);
}
