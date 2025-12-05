using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentCarX.Application.DTOs.Auth;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Services.NotificationService.Flags;

namespace RentCarX.Application.CQRS.Commands.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponseDto>
{
    private readonly UserManager<User> _userManager;
    private readonly IEnumerable<INotificationSender> _senders;
    private readonly NotificationFeatureFlags _featureFlags;
    private readonly IConfiguration _configuration; 
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IOptions<NotificationFeatureFlags> featureFlags,
        UserManager<User> userManager, 
        IEnumerable<INotificationSender> senders,
        IConfiguration configuration,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _senders = senders;
        _configuration = configuration;
        _featureFlags = featureFlags.Value;
        _logger = logger; 
    }

    public async Task<ForgotPasswordResponseDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user?.Email is null)
        {
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
            return new ForgotPasswordResponseDto { ResetLink = "If a user with that email exists, a password reset link has been sent." };
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(resetToken);

        var frontendUrl = _configuration["FrontendUrl"];
        if (string.IsNullOrEmpty(frontendUrl))
        {
            _logger.LogWarning("FrontendUrl is not configured in appsettings. Using fallback 'https://frontend.com'.");
            frontendUrl = "https://frontend.com"; 
        }

        var resetLink = $"{frontendUrl.TrimEnd('/')}/reset-password?userId={user.Id}&token={encodedToken}";

        try
        {
            var emailSubject = "RentCarX password reset instructions";
            var emailBody = $@"
                <html>
                <body>
                    <p>Hello {user.UserName ?? user.Email},</p>
                    <p>We have received a request to reset your password for your RentCarX account.</p>
                    <p>To reset your password, click the link below:</p>
                    <p><a href='{resetLink}'>Reset your password</a></p>
                    <p>This link will expire after a specified period of time (e.g. 24 hours) and can only be used once.</p>
                    <p>If you have not requested a password reset, please ignore this message.</p>
                </body>
                </html>";

            if(_featureFlags.UseSmtpProtocol)
            {
                var smtp = _senders.First(s => s.StrategyName.Equals(NotificationStrategyOptions.Smtp));
                await smtp.SendNotificationAsync(emailSubject, emailBody, cancellationToken, user.Email);
            }

            Console.WriteLine($"Password reset email sent to {user.Email}. Link: {resetLink}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
        }

        return new ForgotPasswordResponseDto { ResetLink = resetLink };
    }
}
