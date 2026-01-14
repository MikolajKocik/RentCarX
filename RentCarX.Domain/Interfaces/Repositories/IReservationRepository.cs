using Microsoft.EntityFrameworkCore.Storage;
using RentCarX.Domain.Models;
using RentCarX.Domain.Models.Enums;
using System.Linq.Expressions;

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
        Task<List<Guid>> GetCarIdsWithActiveReservationAsync(DateTime currentTime, CancellationToken cancellationToken);
        Task UpdateReservationStatusAsync(
            Expression<Func<Reservation, bool>> predicate,
            ReservationStatus newStatus,
            CancellationToken cancellationToken);
    }
}
