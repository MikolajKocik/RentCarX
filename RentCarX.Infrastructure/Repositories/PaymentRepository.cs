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
            .FirstOrDefaultAsync(p => p.StripeCheckoutSessionId == sessionId, cancellationToken);
    }
}
