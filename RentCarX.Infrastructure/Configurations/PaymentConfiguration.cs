using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarX.Domain.Models.Stripe;

namespace RentCarX.Infrastructure.Configurations
{
    public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasOne(i => i.Item)
                .WithMany()
                .HasForeignKey(p => p.ItemId);

            builder.Property(p => p.Amount)
                .HasPrecision(12, 2);
        }
    }
}
