using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Models;

namespace RentCarX.Infrastructure.Data
{
    public class RentCarX_DbContext : IdentityDbContext<IdentityUser>, IRentCarX_DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<User> Users { get; set; }

        public RentCarX_DbContext(DbContextOptions<RentCarX_DbContext> options)
        : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin<string>>()
                    .HasKey(login => new { login.LoginProvider, login.ProviderKey });

            // assembly reference to all configurations classes in solution

            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
