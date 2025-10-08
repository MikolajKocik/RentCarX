namespace RentCarX.Application.DTOs.Auth
{
    public sealed record LoginDto
    {
        public required string Email { get; init; } 
        public required string Password { get; init; } 
    }
}
