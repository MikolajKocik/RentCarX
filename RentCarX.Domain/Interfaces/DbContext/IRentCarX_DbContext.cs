using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RentCarX.Domain.Models;

namespace RentCarX.Domain.Interfaces.DbContext
{
    public interface IRentCarX_DbContext
    {
        DbSet<Car> Cars { get; set; }
        DbSet<Reservation> Reservations { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        DatabaseFacade Database { get; }
    }
}
