using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Domain.Exceptions;

namespace RentCarX.Application.CQRS.Commands.Auth.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        ITokenBlacklistService tokenBlacklistService,
        IHttpContextAccessor accessor,
        ILogger<LogoutCommandHandler> logger
        )
    {
        _tokenBlacklistService = tokenBlacklistService;
        _accessor = accessor;
        _logger = logger;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var token = _accessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                throw new BadRequestException("Token not found.");
            }

            await _tokenBlacklistService.BlacklistTokenAsync(token);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Internal server error occurred");
            throw;
        }
    }
}
