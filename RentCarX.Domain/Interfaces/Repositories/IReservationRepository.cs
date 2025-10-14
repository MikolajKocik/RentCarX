using Microsoft.EntityFrameworkCore.Storage;
using RentCarX.Domain.Models; 

namespace RentCarX.Domain.Interfaces.Repositories
{
    public interface IReservationRepository
    {
        Task Create(Reservation reservation, CancellationToken cancellation);

        IQueryable<Reservation> GetAll();
        IQueryable<Reservation> GetDeletedReservations(Guid id);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellation);

        Task<Reservation?> GetReservationByIdAsync(Guid id, CancellationToken cancellation);

        Task<bool> HasOverlappingReservationAsync(Guid carId, DateTime startDate, DateTime endDate, CancellationToken cancellation);

        Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task SaveToDatabase(CancellationToken cancellationToken);
        Task<IEnumerable<Reservation>> GetUserReservationsAsync(Guid userId, CancellationToken cancellationToken);
    }
}
