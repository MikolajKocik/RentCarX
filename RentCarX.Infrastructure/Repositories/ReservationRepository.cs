using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using RentCarX.Domain.Models; 
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Infrastructure.Data;

namespace RentCarX.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly RentCarX_DbContext _context;
    private readonly ILogger<ReservationRepository> _logger;

    public ReservationRepository(RentCarX_DbContext context, UserManager<IdentityUser> userManager,
        ILogger<ReservationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Create(Reservation reservation, CancellationToken cancellation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellation);
    }

    
    public async Task<ICollection<Reservation>> GetUserReservations(string userId, CancellationToken cancellation)
        => await _context.Reservations
               .Where(r => r.UserId == userId)
               .Include(r => r.Car)
               .ToListAsync(cancellation);

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellation)
        => await _context.Database.BeginTransactionAsync(cancellation);

    public async Task<Reservation?> GetReservationByIdAsync(Guid id, CancellationToken cancellation)
    {
        _logger.LogInformation($"Executing SQL query for ReservationId: {id}"); 

        return await _context.Reservations
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.Id == id, cancellation);
    }
    
    public async Task<bool> HasOverlappingReservationAsync(Guid carId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await _context.Reservations.AnyAsync(r =>
                r.CarId == carId &&
                r.EndDate >= startDate && 
                r.StartDate <= endDate, 
                cancellationToken);
    }
}