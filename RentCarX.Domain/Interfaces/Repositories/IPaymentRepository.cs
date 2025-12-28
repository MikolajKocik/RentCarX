using RentCarX.Domain.Models.Stripe;

namespace RentCarX.Domain.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
        Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
        Task<Payment?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
        Task<Payment?> GetByRefundIdAsync(string refundId, CancellationToken cancellationToken = default);
        Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);
        IQueryable<Payment> GetPendingReservations();
        Task<List<Guid>> GetLockedCarIdsAsync(CancellationToken cancellationToken = default);
    }
}
