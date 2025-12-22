using MediatR;
using Microsoft.AspNetCore.Identity;
using RentCarX.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Linq;

namespace RentCarX.Application.CQRS.Commands.Auth.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(UserManager<User> userManager, ILogger<ConfirmEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Find user by id
        User? user = await _userManager.FindByIdAsync(request.userId.ToString());
        if (user == null) throw new NotFoundException(nameof(user), "User not found.");

        var originalToken = request.token ?? string.Empty;

        // Try multiple token variants to tolerate encoding differences from client
        var variants = new List<string>
        {
            originalToken,
            WebUtility.UrlDecode(originalToken),
            (originalToken ?? string.Empty).Replace(' ', '+')
        }
        .Where(v => !string.IsNullOrEmpty(v))
        .Distinct()
        .ToList();

        foreach (var variant in variants)
        {
            try
            {
                var result = await _userManager.ConfirmEmailAsync(user, variant);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Email confirmed for user {UserId} using token variant.", request.userId);
                    return true;
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogDebug("ConfirmEmail attempt failed for user {UserId} with token variant '{TokenVariant}': {Errors}", request.userId, variant, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while confirming email for user {UserId} with token variant.", request.userId);
            }
        }

        _logger.LogWarning("All ConfirmEmail attempts failed for user {UserId}. Tried {Count} variants.", request.userId, variants.Count);
        return false;
    }
}
