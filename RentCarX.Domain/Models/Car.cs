namespace RentCarX.Domain.Models
{
    public sealed class Car
    {
        private int _isAvailableFlag = 1;

        public Guid Id { get; set; }
        public string Brand { get; set; } = default!;
        public string Model { get; set; } = default!;
        public int Year { get; set; }
        public string FuelType { get; set; } = default!;
        public decimal PricePerDay { get; set; }

        // 1 = true | 0 = false
        [System.Text.Json.Serialization.JsonIgnore]
        public int IsAvailableFlag
        {
            get => this._isAvailableFlag;
            set => this._isAvailableFlag = value;
        }

        public bool IsAvailable
        {
            get => IsAvailableFlag == 1;
            set => IsAvailableFlag = value ? 1 : 0;
        }

        // Stripe integration
        public string? StripeProductId { get; set; }
        public string? StripePriceId { get; set; }

        public List<Reservation> Reservations { get; set; } = new();

        // if not reserved change to reserved with flag and return a result before
        public bool TryReserve()
            => Interlocked.CompareExchange(ref _isAvailableFlag, 0, 1) == 1;
    }
}
