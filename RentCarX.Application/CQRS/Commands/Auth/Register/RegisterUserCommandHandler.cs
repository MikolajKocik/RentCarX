using MediatR;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using RentCarX.Application.DTOs.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using RentCarX.Application.Interfaces.Services.EmailService;

namespace RentCarX.Application.CQRS.Commands.Auth.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponseDto>
    {
        private readonly IJwtTokenService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            UserManager<User> userManager,
            IJwtTokenService jwtService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RegisterUserResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _userManager.FindByEmailAsync(request.Dto.Email);
            if (userExists != null)
                throw new ConflictException("User with this email already exists.", $"Email: {request.Dto.Email}");

            var user = new User
            {
                UserName = request.Dto.Username,
                Email = request.Dto.Email,
                EmailConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user, request.Dto.Password);
            if (!createResult.Succeeded)
                throw new Exception($"Create failed: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(user, "User");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token); 
            // TODO url front-web
            var frontendUrl = _configuration["FrontendUrl"] ?? "https://yourfrontend.com";

            var confirmationUrl = $"{frontendUrl.TrimEnd('/')}/confirm-email?userId={user.Id}&token={encodedToken}";
            //
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

                await _emailService.SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
            }

            var jwtToken = await _jwtService.GenerateToken(user);

            _logger.LogInformation("Generated token: {Token}", token);
            _logger.LogInformation("Encoded token: {EncodedToken}", encodedToken);
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
