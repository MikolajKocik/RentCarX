using RentCarX.Application.DTOs.Stripe;

namespace RentCarX.Domain.Interfaces.Services.Stripe
{
    public interface IPaymentService
    {
        Task<string> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest sessionRequest, CancellationToken cancellationToken);
        Task HandleCheckoutSessionCompletedAsync(string sessionId, string? paymentIntentId = null, string? customerId = null, CancellationToken cancellationToken = default);
        Task HandleRefundSucceededAsync(string refundId, string? chargeId = null, long? amount = null, string? currency = null, CancellationToken cancellationToken = default);
    }
}
