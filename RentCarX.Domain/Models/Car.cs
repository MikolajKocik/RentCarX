namespace RentCarX.Domain.Models
{
    public class Car
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = default!;
        public string Model { get; set; } = default!;
        public int Year { get; set; }
        public string FuelType { get; set; } = default!;
        public decimal PricePerDay { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Stripe integration
        public string? StripeProductId { get; set; }
        public string? StripePriceId { get; set; }

        public List<Reservation> Reservations { get; set; } = new();
    }
}
