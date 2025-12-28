using Microsoft.Extensions.Logging;
using RentCarX.Domain.Interfaces.Services.Stripe;
using Stripe;
using Stripe.Checkout;

namespace RentCarX.Infrastructure.Services.Stripe;

public class StripeWebhookHandlerImplementation : IStripeWebhookHandler
{
    private readonly ILogger<StripeWebhookHandlerImplementation> _logger;
    private readonly IPaymentService _paymentService;

    public StripeWebhookHandlerImplementation(
        ILogger<StripeWebhookHandlerImplementation> logger,
        IPaymentService paymentService)
    {
        _logger = logger;
        _paymentService = paymentService;
    }

    public async Task HandleEventAsync(Event stripeEvent, CancellationToken cancellationToken = default)
    {
        if (stripeEvent is null) return;

        _logger.LogInformation("Handling Stripe event: {Type}", stripeEvent.Type);

        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                var session = stripeEvent.Data.Object as Session;
                if (session is not null)
                {
                    _logger.LogInformation("Processing successful payment for session: {SessionId}", session.Id);

                    await _paymentService.HandleCheckoutSessionCompletedAsync(
                        session.Id,
                        session.PaymentIntentId,
                        session.CustomerId,
                        session .InvoiceId,
                        cancellationToken
                    );
                }
                break;

            case "checkout.session.expired":
                var expiredSession = stripeEvent.Data.Object as Session;
                if (expiredSession is not null)
                {
                    _logger.LogInformation("Checkout session expired: {Id}", expiredSession.Id);

                    await _paymentService.HandleCheckoutSessionExpiredAsync(expiredSession.Id, cancellationToken);
                }
                break;

            case "payment_intent.payment_failed":
                var intent = stripeEvent.Data.Object as PaymentIntent;
                if (intent is not null)
                {
                    _logger.LogWarning("Payment failed for intent: {Id}. Reason: {Reason}",
                        intent.Id, intent.LastPaymentError?.Message);

                    await _paymentService.HandlePaymentFailedAsync(intent.Id, cancellationToken);
                }
                break;

            case "charge.refunded":
            case "charge.dispute.closed":
            case "refund.updated":
            case "refund.succeeded":
                try
                {
                    var refund = stripeEvent.Data.Object as Refund;
                    if (refund is not null)
                    {
                        object? chargeObj = refund.Charge;
                        string? chargeId = null;

                        if (chargeObj is string s)
                        {
                            chargeId = s;
                        }
                        else if (chargeObj is Charge c)
                        {
                            chargeId = c.Id;
                        }
                        else
                        {
                            chargeId = chargeObj?.ToString();
                        }

                        long amount = refund.Amount;
                        string currency = refund.Currency;

                        await _paymentService.HandleRefundSucceededAsync(
                            refund.Id,
                            chargeId,
                            amount, 
                            currency,
                            cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling refund event");
                }
                break;

            default:
                _logger.LogInformation("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                break;
        }
    }
}
