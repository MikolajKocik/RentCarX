using MediatR;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Application.Interfaces.PasswordHasher; 
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories; 

namespace RentCarX.Application.CQRS.Commands.Auth.Login
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtService;
        private readonly IPasswordHasher _passwordHasher; 

        public LoginUserCommandHandler(IUserRepository userRepository, IJwtTokenService jwtService, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Dto.Email, cancellationToken);

            if (user is null || !_passwordHasher.VerifyPasswordHash(request.Dto.Password, user.CustomPasswordHash, user.CustomPasswordSalt)) 
            {
                throw new UnauthorizedException("Invalid credentials."); 
            }

            return await _jwtService.GenerateToken(user); 
        }
    }
}
