using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentCarX.Application.DTOs.Auth;
using RentCarX.Application.Helpers;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Application.Interfaces.Services.NotificationStrategy;
using RentCarX.Application.Services.NotificationService.Flags;
using RentCarX.Domain.Exceptions;
using System.Net;

namespace RentCarX.Application.CQRS.Commands.Auth.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponseDto>
    {
        private readonly IJwtTokenService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly IEnumerable<INotificationSender> _senders;
        private readonly IConfiguration _configuration;
        private readonly NotificationFeatureFlags _flags;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            UserManager<User> userManager,
            IJwtTokenService jwtService,
            IEnumerable<INotificationSender> senders,
            IConfiguration configuration,
            ILogger<RegisterUserCommandHandler> logger,
            IOptions<NotificationFeatureFlags> flags)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _senders = senders;
            _configuration = configuration;
            _logger = logger;
            _flags = flags.Value;
        }

        public async Task<RegisterUserResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Validate request
            if (request?.Dto is null)
                throw new ArgumentNullException(nameof(request), "Register request cannot be null.");

            // Check user by email
            var userByEmail = await _userManager.FindByEmailAsync(request.Dto.Email);
            if (userByEmail != null)
                throw new ConflictException("User with this email already exists.", $"Email: {request.Dto.Email}");

            // Check user by username
            var userByName = await _userManager.FindByNameAsync(request.Dto.Username);
            if (userByName != null)
                throw new ConflictException("User with this username already exists.", $"Username: {request.Dto.Username}");

            var user = new User
            {
                UserName = request.Dto.Username,
                Email = request.Dto.Email,
                EmailConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user, request.Dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to create user {Email}: {Errors}", request.Dto.Email, errors);
                throw new Exception($"Create failed: {errors}");
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to add role to user {UserId}: {Errors}", user.Id, errors);
            }

            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(emailToken);

            // frontend url from configuration (fallback provided)
            var frontendUrl = _configuration["FrontendUrl"] ?? "https://yourfrontend.com";
            var confirmationUrl = $"{frontendUrl.TrimEnd('/')}/confirm-email?userId={user.Id}&token={encodedToken}";

            try
            {
                var subject = "RentCarX email address confirmation";
                var body = $@"
                <html>
                    <body>
                        <p>Hello {user.UserName},</p>
                        <p>Thank you for registering with RentCarX.</p>
                        <p>Click on the link to confirm your email address:</p>
                        <p><a href='{confirmationUrl}'>Confirm your email address</a></p>
                        <p>If you are not the one who registered, please ignore this message.</p>
                    </body>
                </html>";

                if (_flags.UseSmtpProtocol)
                {
                    var smtp = _senders.FirstOrDefault(s => s.StrategyName == NotificationStrategyOptions.Smtp);
                    if (smtp != null)
                    {
                        await smtp.SendNotificationAsync(subject, body, cancellationToken, user.Email);
                        _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
                    }
                    else
                    {
                        _logger.LogWarning("SMTP notification sender not configured; skipping email send for {Email}", user.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
            }

            // generate JWT
            var jwtToken = await _jwtService.GenerateToken(user);

            // Log helpful info but avoid printing raw confirmation token
            _logger.LogInformation("Generated JWT for user {UserId}", user.Id);
            _logger.LogInformation("Confirmation link: {Link}", confirmationUrl);

            return new RegisterUserResponseDto
            {
                JwtToken = jwtToken,
                UserId = user.Id,
                ConfirmationLink = confirmationUrl
            };
        }
    }
}
