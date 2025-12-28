namespace RentCarX.Application.DTOs.Reservation;

public sealed record ReservationDeadlineDto
{
    public Guid Id { get; init; }
    public Guid CarId { get; init; }
    public string CarName { get; init; } = default!;
    public DateTime Deadline { get; init; }
}
