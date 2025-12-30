namespace RentCarX.Application.Interfaces.JWT;

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(string token);
    Task<bool> IsTokenBlacklistedAsync(string token);
}
