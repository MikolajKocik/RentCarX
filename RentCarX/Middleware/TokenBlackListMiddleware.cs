using RentCarX.Application.Interfaces.JWT;

namespace RentCarX.Presentation.Middleware;

public sealed class TokenBlackListMiddleware
{
    private readonly RequestDelegate _next;

    public TokenBlackListMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService tokenBlacklistService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null && await tokenBlacklistService.IsTokenBlacklistedAsync(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token has been invalidated.");
            return;
        }

        await _next(context);
    }
}
