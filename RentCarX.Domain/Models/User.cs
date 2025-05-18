using Microsoft.AspNetCore.Identity;
using RentCarX.Domain.Models;

public class User : IdentityUser<Guid>
{
    public List<Reservation> Reservations { get; set; } = new List<Reservation>();

    public User() : base() { }
}
