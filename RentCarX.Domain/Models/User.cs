namespace RentCarX.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public byte[] PasswordHash { get; set; } = default!;
        public byte[] PasswordSalt { get; set; } = default!;
        public string Role { get; set; } = "User";

        public List<Reservation> Reservations { get; set; } = new List<Reservation>();

    }

}
