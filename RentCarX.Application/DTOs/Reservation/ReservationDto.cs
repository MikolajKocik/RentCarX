using Newtonsoft.Json;
using RentCarX.Domain.Models.Enums;
using System.Text.Json.Serialization;

namespace RentCarX.Application.DTOs.Reservation;

public class ReservationDto
{
    public Guid Id { get; set; }
    public Guid CarId { get; set; }
    public string CarName { get; set; } = default!;
    public string? CarImageUrl { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalCost { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;
    public bool IsPaid { get; set; }
    public int DurationDays { get; set; }
}
