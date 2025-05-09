using Microsoft.AspNetCore.Identity;
using RentCarX.Domain.Models;

public class User : IdentityUser<Guid>
{
    public byte[] CustomPasswordHash { get; set; } = default!;
    public byte[] CustomPasswordSalt { get; set; } = default!;

    public List<Reservation> Reservations { get; set; } = new List<Reservation>();

    public User() : base() { }
}
