using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Models;
using RentCarX.Domain.Models.Stripe;
using RentCarX.Infrastructure.Data.Schemas;
using RentCarX.Infrastructure.Settings;

#pragma warning disable CS8625

namespace RentCarX.Infrastructure.Data;

public sealed class RentCarX_DbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IRentCarX_DbContext
{
    private readonly IdentityAdminRole _adminRole;

    public DbSet<Car> Cars { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    internal DbSet<Item> Items { get; set; }
    internal DbSet<Payment> Payments { get; set; }
    internal DbSet<Refund> Refunds { get; set; }
    internal DbSet<StripeCustomer> StripeCustomers { get; set; }
    public override DbSet<User> Users { get; set; }

    public RentCarX_DbContext(DbContextOptions<RentCarX_DbContext> options, IOptions<IdentityAdminRole> adminRole)
    : base(options)
    {
        _adminRole = adminRole.Value;
    }

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

        // query filter for soft deleted users
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => !u.IsDeleted);

        // assembly reference to all configurations classes in solution
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

        #region SeedAdminRole
        Guid adminRoleId = new Guid("99000000-0000-0000-0000-00000000AAAA");
        Guid adminUserId = new Guid("99000000-0000-0000-0000-00000000DDDD");

        modelBuilder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN"
            }
        );

        var hasher = new PasswordHasher<User>();

        modelBuilder.Entity<User>().HasData(
            new User 
            {
                Id = adminUserId,
                UserName = "admin@rentcarx.com",
                NormalizedUserName = "ADMIN@RENTCARX.COM",
                Email = "admin@rentcarx.com",
                NormalizedEmail = "ADMIN@RENTCARX.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, _adminRole.Password),
                SecurityStamp = Guid.NewGuid().ToString(),
                IsDeleted = false 
            }
        );

        modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(
            new IdentityUserRole<Guid>
            {
                RoleId = adminRoleId,
                UserId = adminUserId
            }
        );
        #endregion
    }
}

#pragma warning restore CS8625
