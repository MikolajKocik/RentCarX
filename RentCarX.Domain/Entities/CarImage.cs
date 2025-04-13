namespace RentCarX.Domain.Entities
{
    public class CarImage
    {
        public int Id { get; set; }
        public string FileName { get; set; } = default!;
        public string Path { get; set; } = default!;

        public int CarId { get; set; }
        public Car Car { get; set; } = default!;
    }
}
