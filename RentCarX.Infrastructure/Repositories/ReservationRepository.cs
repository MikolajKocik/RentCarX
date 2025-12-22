using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using RentCarX.Domain.Models;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Infrastructure.Data;

namespace RentCarX.Infrastructure.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly RentCarX_DbContext _context;
    private readonly ILogger<ReservationRepository> _logger;

    public ReservationRepository(RentCarX_DbContext context, ILogger<ReservationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Create(Reservation reservation, CancellationToken cancellationToken)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<Reservation> GetDeletedReservations(Guid id)
        => _context.Reservations
            .Where(r => r.Id == id && r.IsDeleted)
            .Include(r => r.Car)
            .AsNoTracking()
            .AsQueryable();
    public IQueryable<Reservation> GetAll()
        => _context.Reservations
               .IgnoreQueryFilters()
               .Include(r => r.Car)
               .AsNoTracking()
               .AsQueryable();

    public async Task<IEnumerable<Reservation>> GetUserReservationsAsync(Guid userId, CancellationToken cancellationToken)
        => await _context.Reservations
            .Where(r => r.UserId == userId)
            .Include(r => r.Car)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<List<Guid>> GetCarIdsWithActiveReservationAsync(DateTime currentTime, CancellationToken cancellationToken)
        => await _context.Reservations
             .Where(r => r.EndDate >= currentTime && r.StartDate <= currentTime)
             .Select(r => r.CarId)
             .Distinct()
             .ToListAsync(cancellationToken);

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        => await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task<Reservation?> GetReservationByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<bool> HasOverlappingReservationAsync(Guid carId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await _context.Reservations.AnyAsync(r =>
                r.CarId == carId &&
                r.EndDate >= startDate &&
                r.StartDate <= endDate,
                cancellationToken);
    }

    public async Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Reservation? reservation = await _context.Reservations.FindAsync(id, cancellationToken);

        _context.Reservations.Remove(reservation!);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveToDatabase(CancellationToken cancellationToken)
        => await _context.SaveChangesAsync(cancellationToken);
}