using Microsoft.AspNetCore.Identity;
using RentCarX.Domain.Models;
using RentCarX.Domain.Models.Stripe;

public sealed class User : IdentityUser<Guid>
{
    public List<Reservation> Reservations { get; set; } = new List<Reservation>();

    public User() : base() { }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public StripeCustomer? StripeCustomer { get; set; }
    public ICollection<Payment>? Payments { get; set; }
}
