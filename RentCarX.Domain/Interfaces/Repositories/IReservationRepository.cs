using Microsoft.EntityFrameworkCore.Storage;
using RentCarX.Domain.Models; 

namespace RentCarX.Domain.Interfaces.Repositories
{
    public interface IReservationRepository
    {
        Task Create(Reservation reservation, CancellationToken cancellation);

        Task<ICollection<Reservation>> GetUserReservations(string userId, CancellationToken cancellation);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellation);

        Task<Reservation?> GetReservationByIdAsync(Guid id, CancellationToken cancellation);

        Task<bool> HasOverlappingReservationAsync(Guid carId, DateTime startDate, DateTime endDate, CancellationToken cancellation);
    }
}
