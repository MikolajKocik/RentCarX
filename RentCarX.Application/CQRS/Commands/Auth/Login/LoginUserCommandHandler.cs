using MediatR;
using Microsoft.AspNetCore.Identity;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Domain.Exceptions;

namespace RentCarX.Application.CQRS.Commands.Auth.Login
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
    {
        private readonly IJwtTokenService _jwtService;
        private readonly UserManager<User> _userManager;

        public LoginUserCommandHandler(UserManager<User> userManager, IJwtTokenService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Dto.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Dto.Password))
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (!user.EmailConfirmed)
            {
                throw new EmailNotConfirmedException("Please confirm your email address before logging in.");
            }

            return await _jwtService.GenerateToken(user); 
        }
    }
}
