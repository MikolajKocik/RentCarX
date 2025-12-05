using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Models;
using RentCarX.Domain.Models.Stripe;
using RentCarX.Infrastructure.Data.Schemas;

namespace RentCarX.Infrastructure.Data
{
    public sealed class RentCarX_DbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IRentCarX_DbContext
    {
        internal DbSet<Car> Cars { get; set; }
        internal DbSet<Reservation> Reservations { get; set; }
        internal DbSet<Item> Items { get; set; }
        internal DbSet<Payment> Payments { get; set; }
        internal DbSet<Refund> Refunds { get; set; }
        internal DbSet<StripeCustomer> StripeCustomers { get; set; }
        public override DbSet<User> Users { get; set; }

        public RentCarX_DbContext(DbContextOptions<RentCarX_DbContext> options)
        : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(DefaultSchema.RentCarXDefault);

            modelBuilder.Entity<IdentityUserLogin<string>>()
                    .HasKey(login => new { login.LoginProvider, login.ProviderKey });

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>();

            // query filter for soft deleted reservations
            modelBuilder.Entity<Reservation>()
                .HasQueryFilter(f => !f.IsDeleted);

            // assembly reference to all configurations classes in solution
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
