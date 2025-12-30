using RentCarX.Application.DTOs.Reservation;

namespace RentCarX.Application.DTOs.Car;

public record CarStatusDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
    public List<ReservationBriefDto>? ActiveReservations { get; init; } 
}
