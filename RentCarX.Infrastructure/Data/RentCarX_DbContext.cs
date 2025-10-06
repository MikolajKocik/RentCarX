using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentCarX.Domain.Models;
using RentCarX.Domain.Models.Stripe;
using RentCarX.Infrastructure.Data.Schemas;

namespace RentCarX.Infrastructure.Data
{
    public class RentCarX_DbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid> 
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<StripeCustomer> StripeCustomers { get; set; }
        public override DbSet<User> Users { get; set; }

        public RentCarX_DbContext(DbContextOptions<RentCarX_DbContext> options)
        : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(DefaultSchema.RentCarXDefault);

            modelBuilder.Entity<IdentityUserLogin<string>>()
                    .HasKey(login => new { login.LoginProvider, login.ProviderKey });

            // assembly reference to all configurations classes in solution

            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
