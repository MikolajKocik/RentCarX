using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarX.Domain.Models;

namespace RentCarX.Infrastructure.Configurations 
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            // Relation 1-* between Car and Reservations

            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.Car)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CarId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Property(r => r.TotalCost)
             .HasPrecision(18, 2);
        }
    }
}
