using RentCarX.Domain.Models;

namespace RentCarX.Domain.Interfaces.Services.Stripe
{
    public interface IPaymentService
    {
        Task<string> CreateCheckoutSessionAsync(Reservation reservation, CancellationToken cancellationToken);
    }
}
