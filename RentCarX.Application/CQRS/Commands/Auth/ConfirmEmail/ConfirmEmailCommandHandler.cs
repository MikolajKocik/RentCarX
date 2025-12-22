using MediatR;
using Microsoft.AspNetCore.Identity;
using RentCarX.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace RentCarX.Application.CQRS.Commands.Auth.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, string>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(UserManager<User> userManager, ILogger<ConfirmEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<string> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Find user by id
        User? user = await _userManager.FindByIdAsync(request.userId.ToString());
        if (user == null) throw new NotFoundException(nameof(user), "User not found.");

        var fixedToken = request.token.Replace(" ", "+");
        var result = await _userManager.ConfirmEmailAsync(user, fixedToken);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("ConfirmEmail attempt failed for user {UserId}: {Errors}", request.userId, errors);
            return errors;
        }

        _logger.LogInformation("Email confirmed for user {UserId} using token variant.", request.userId);

        return result.ToString();
    }
}
