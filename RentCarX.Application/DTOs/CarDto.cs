using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.DTOs
{
    public class CarDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = default!;
        public string Model { get; set; } = default!;
        public int Year { get; set; }
        public string FuelType { get; set; } = default!;
        public decimal PricePerDay { get; set; }
        public bool IsAvailable { get; set; }
    }
}
