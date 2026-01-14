using Microsoft.AspNetCore.Http;

namespace RentCarX.Application.DTOs.Car
{
    public class EditCarDto
    {
        public string? Brand { get; set; } 
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? FuelType { get; set; }
        public decimal? PricePerDay { get; set; }
        public bool IsAvailable { get; set; }
        public IFormFile? Image { get; set; }
    }
}
