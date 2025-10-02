using RentCarX.Application.DTOs.Car;

namespace RentCarX.Application.DTOs.Stripe
{
    public sealed record CreateCheckoutSessionRequest
    {
        public Guid CarId { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public Guid UserId { get; init; }
        public decimal TotalCost { get; init; }
        public Guid ReservationId { get; init; }
    }
}
