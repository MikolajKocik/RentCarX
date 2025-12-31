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
                var charge = stripeEvent.Data.Object as Charge;
                if (charge != null)
                {
                    string refundId = charge.Refunds?.Data?.FirstOrDefault()?.Id ?? "unknown";

                    await _paymentService.HandleRefundSucceededAsync(
                        refundId: refundId,
                        sendEmail: true,
                        paymentIntentId: charge.PaymentIntentId, 
                        amount: charge.AmountRefunded,           
                        currency: charge.Currency,
                        ct: cancellationToken);
                }
                break;
            case "charge.dispute.closed":      
            case "refund.succeeded":
            case "refund.updated":
                var refund = stripeEvent.Data.Object as Refund;
                if (refund != null)
                {
                    await _paymentService.HandleRefundSucceededAsync(
                        refundId: refund.Id,
                        sendEmail: false,
                        paymentIntentId: refund.PaymentIntentId, 
                        amount: refund.Amount,
                        currency: refund.Currency,
                        ct: cancellationToken
                    );
                }
                break;

            default:
                _logger.LogInformation("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                break;
        }
    }
}
