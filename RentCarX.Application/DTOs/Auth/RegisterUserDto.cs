namespace RentCarX.Application.DTOs.Auth
{
    public sealed record RegisterUserDto
    {
        public required string Username { get; init; } 
        public required string Email { get; init; } 
        public required string Password { get; init; } 
    }
}
