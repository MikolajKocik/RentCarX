using RentCarX.Application.DTOs.Car;

namespace RentCarX.Application.DTOs.Stripe
{
    public sealed class CreateCheckoutSessionRequest
    {
        public Guid ReservationId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }
}
