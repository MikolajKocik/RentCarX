using RentCarX.Domain.Models.Enums;

namespace RentCarX.Application.DTOs.Reservation;

public record ReservationBriefDto
{
    public Guid Id { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } 
    public bool IsPaid { get; init; }
}
