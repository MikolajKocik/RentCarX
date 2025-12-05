namespace RentCarX.Domain.Models
{
    public sealed class Car
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = default!;
        public string Model { get; set; } = default!;
        public int Year { get; set; }
        public string FuelType { get; set; } = default!;
        public decimal PricePerDay { get; set; }

        // 1 = true | 0 = false
        public int IsAvailableFlag = 1;

        // Stripe integration
        public string? StripeProductId { get; set; }
        public string? StripePriceId { get; set; }

        public List<Reservation> Reservations { get; set; } = new();
    }
}
