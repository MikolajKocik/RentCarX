namespace RentCarX.Application.Interfaces.JWT
{
    public interface IJwtTokenService
    {
        Task<string> GenerateToken(User user); 
    }
}
