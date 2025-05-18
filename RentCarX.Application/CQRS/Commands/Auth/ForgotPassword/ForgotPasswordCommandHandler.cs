using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentCarX.Application.DTOs.Auth;
using RentCarX.Application.Interfaces.EmailService;

namespace RentCarX.Application.CQRS.Commands.Auth.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponseDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration; 
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(UserManager<User> userManager, IEmailService emailService,
            IConfiguration configuration, ILogger<ForgotPasswordCommandHandler> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration; 
            _logger = logger; 
        }

        public async Task<ForgotPasswordResponseDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
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
                var emailSubject = "Instrukcja resetowania hasła w RentCarX";
                var emailBody = $@"
                <html>
                <body>
                    <p>Witaj {user.UserName ?? user.Email},</p>
                    <p>Otrzymaliśmy prośbę o zresetowanie hasła do Twojego konta w RentCarX.</p>
                    <p>Aby zresetować hasło, kliknij w poniższy link:</p>
                    <p><a href='{resetLink}'>Zresetuj swoje hasło</a></p>
                    <p>Ten link wygaśnie po określonym czasie (np. 24 godziny) i może być użyty tylko raz.</p>
                    <p>Jeśli nie prosiłeś o zresetowanie hasła, zignoruj tę wiadomość.</p>
                </body>
                </html>";

                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
                Console.WriteLine($"Password reset email sent to {user.Email}. Link: {resetLink}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            }

            return new ForgotPasswordResponseDto { ResetLink = resetLink };
        }
    }
}
