using RentCarX.Domain.Models;

namespace RentCarX.Application.Interfaces.JWT
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
