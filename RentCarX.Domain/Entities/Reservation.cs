namespace RentCarX.Domain.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsConfirmed { get; set; }
        public decimal TotalCost { get; set; }

        public string UserId { get; set; } = default!; // Identity user

        public int CarId { get; set; }
        public Car Car { get; set; } = default!;
    }
}
