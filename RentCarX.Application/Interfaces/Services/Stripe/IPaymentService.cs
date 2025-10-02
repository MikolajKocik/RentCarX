using RentCarX.Application.DTOs.Stripe;

namespace RentCarX.Domain.Interfaces.Services.Stripe
{
    public interface IPaymentService
    {
        Task<string> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest sessionRequest, CancellationToken cancellationToken);
    }
}
