using RentCarX.Domain.Models.Enums;

namespace RentCarX.Domain.Models
{
    public sealed class Reservation
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public Guid UserId { get; set; } 

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalCost { get; set; }
        public bool IsPaid { get; set; }
        public bool IsDeleted { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public Car Car { get; set; } = default!;

        public User User { get; set; } = default!;
        public int PricePerDay { get; set; }
    }
}
