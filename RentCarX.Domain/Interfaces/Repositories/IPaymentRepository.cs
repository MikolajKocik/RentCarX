using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RentCarX.Domain.Models.Stripe;

namespace RentCarX.Domain.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
        Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
        Task<Payment?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    }
}
