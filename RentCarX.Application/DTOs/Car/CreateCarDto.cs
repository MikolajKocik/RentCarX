using Microsoft.AspNetCore.Http;

namespace RentCarX.Application.DTOs.Car
{
    public class CreateCarDto
    {
        public string Brand { get; set; } = default!;
        public string Model { get; set; } = default!;
        public int Year { get; set; }
        public string FuelType { get; set; } = default!;
        public decimal PricePerDay { get; set; }
        public bool IsAvailable { get; set; } 
        public IFormFile? Image { get; set; }
    }
}
