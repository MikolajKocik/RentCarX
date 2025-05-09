using MediatR;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Application.Interfaces.PasswordHasher;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;
namespace RentCarX.Application.CQRS.Commands.Auth.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
    {
        private readonly IUserRepository _userRepository; 
        private readonly IJwtTokenService _jwtService;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserCommandHandler(IUserRepository userRepository, IJwtTokenService jwtService, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _userRepository.EmailExistsAsync(request.Dto.Email, cancellationToken);
            if (userExists)
            {
                throw new ConflictException("User with this email already exists.", typeof(User).ToString()); 
            }

            _passwordHasher.CreatePasswordHash(request.Dto.Password, out var hash, out var salt);

            var user = new User
            {
                Username = request.Dto.Username,
                Email = request.Dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            await _userRepository.CreateUserAsync(user, cancellationToken);

            return _jwtService.GenerateToken(user);
        }
    }
}