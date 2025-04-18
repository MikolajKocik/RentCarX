namespace RentCarX.Domain.Models
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public Guid UserId { get; set; } 

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalCost { get; set; }

        public Car Car { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
    }

}
