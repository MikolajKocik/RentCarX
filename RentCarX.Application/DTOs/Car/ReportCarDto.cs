using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.DTOs.Car;

public sealed record ReportCarDto
{
    public Guid CarId { get; init; }
    public string Model { get; init; } = string.Empty;
    public decimal PricePerDay { get; init; }
    public int TotalReservations { get; init; }
    public decimal Revenue { get; init; }
}
