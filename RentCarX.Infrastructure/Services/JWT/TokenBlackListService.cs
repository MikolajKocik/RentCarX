using Microsoft.Extensions.Caching.Memory;
using RentCarX.Application.Interfaces.JWT;
using System.IdentityModel.Tokens.Jwt;

namespace RentCarX.Infrastructure.Services.JWT;

public sealed class TokenBlackListService : ITokenBlacklistService
{
    private readonly IMemoryCache _memoryCache;

    public TokenBlackListService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;   
    }

    public Task BlacklistTokenAsync(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

        string jti = jwtToken.Id;
        DateTime expiration = jwtToken.ValidTo;

        _memoryCache.Set(jti, string.Empty, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        });

        return Task.CompletedTask;
    }

    public Task<bool> IsTokenBlacklistedAsync(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return Task.FromResult(false);

            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);
            string jti = jwtToken.Id;

            if (string.IsNullOrEmpty(jti)) return Task.FromResult(false);

            return Task.FromResult(_memoryCache.TryGetValue(jti, out _));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
