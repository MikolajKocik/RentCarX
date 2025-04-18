using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.DTOs
{
    public class ReservationDto
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public string CarName { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalCost { get; set; }
    }

}
