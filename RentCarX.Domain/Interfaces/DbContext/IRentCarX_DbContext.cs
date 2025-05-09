using Microsoft.EntityFrameworkCore;
using RentCarX.Domain.Models;

namespace RentCarX.Domain.Interfaces.DbContext
{
    public interface IRentCarX_DbContext
    {
        DbSet<Car> Cars { get; set; }
        DbSet<Reservation> Reservations { get; set; }
        DbSet<User> Users { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
