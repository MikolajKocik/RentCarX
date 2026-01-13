using Microsoft.EntityFrameworkCore;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models.Stripe;
using RentCarX.Infrastructure.Data;

#pragma warning disable CS8602

namespace RentCarX.Infrastructure.Repositories;

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

    public async Task<List<Guid>> GetLockedCarIdsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.Payments
            .Include(p => p.Reservation)
            .Where(r =>
                (r.Reservation.StartDate <= now && r.Reservation.EndDate >= now) ||
                (r.Status == PaymentStatus.Pending))
            .Select(r => r.Reservation.CarId)
            .Distinct() 
            .ToListAsync(cancellationToken);
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
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(r => r.StripeRefundId == refundId, cancellationToken);

        if (refund is not null)
            return refund.Payment;

        var paymentByCharge = await _dbContext.Payments
            .Include(p => p.User)
            .FirstOrDefaultAsync(
                p => p.StripePaymentIntentId == refundId ||
                p.StripeCheckoutSessionId == refundId, cancellationToken);

        return paymentByCharge;
    }

    public async Task<Payment?> GetByReservationId(Guid reservationId, CancellationToken cancellationToken)
        => await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.ReservationId == reservationId && p.Status == PaymentStatus.Succeeded, cancellationToken);
    
    public Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default)
        => _dbContext.Payments
            .Include(p => p.User)
            .Include(p => p.Item)
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId, cancellationToken);

    public IQueryable<Payment> GetPendingReservations()
        => _dbContext.Payments
            .Include(p => p.Reservation)
                .ThenInclude(r => r.User)
            .Include(p => p.Reservation)
                .ThenInclude(r => r.Car)
            .Where(p => p.Status == PaymentStatus.Pending);
}

#pragma warning restore CS8602
