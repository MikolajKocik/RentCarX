namespace RentCarX.Application.DTOs.Reservation;

public record ReservationBriefDto
{
    public Guid Id { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
