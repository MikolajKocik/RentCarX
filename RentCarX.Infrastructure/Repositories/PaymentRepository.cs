using Microsoft.EntityFrameworkCore;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models.Stripe;
using RentCarX.Infrastructure.Data;

namespace RentCarX.Infrastructure.Repositories
{
    public sealed class PaymentRepository : IPaymentRepository
    {
        private readonly RentCarX_DbContext _dbContext;

        public PaymentRepository(RentCarX_DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            await _dbContext.Payments.AddAsync(payment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _dbContext.Payments.Update(payment);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<Payment?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
            => _dbContext.Payments
            .Include(i => i.Item)
            .Include(u => u.User)
            .FirstOrDefaultAsync(p => p.StripeCheckoutSessionId == sessionId, cancellationToken);

        public async Task<Payment?> GetByRefundIdAsync(string refundId, CancellationToken cancellationToken = default)
        {
            var refund = await _dbContext.Refunds
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.StripeRefundId == refundId, cancellationToken);

            if (refund is not null)
                return refund.Payment;

            var paymentByCharge = await _dbContext.Payments
                .FirstOrDefaultAsync(
                    p => p.StripePaymentIntentId == refundId ||
                    p.StripeCheckoutSessionId == refundId, cancellationToken);

            return paymentByCharge;
        }

        public Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default)
            => _dbContext.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId, cancellationToken);
    }
}
