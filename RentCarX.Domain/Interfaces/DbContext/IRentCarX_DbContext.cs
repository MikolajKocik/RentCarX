using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
