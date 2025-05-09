using MediatR;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Application.Interfaces.PasswordHasher; 
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Exceptions; 
using Microsoft.AspNetCore.Identity; 

namespace RentCarX.Application.CQRS.Commands.Auth.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
    {
        private readonly IUserRepository _userRepository; 
        private readonly IJwtTokenService _jwtService;
        private readonly IPasswordHasher _passwordHasher; 
        private readonly UserManager<User> _userManager;

        public RegisterUserCommandHandler(IUserRepository userRepository, IJwtTokenService jwtService, IPasswordHasher passwordHasher, UserManager<User> userManager)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _userManager = userManager;
        }

        public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _userRepository.EmailExistsAsync(request.Dto.Email, cancellationToken);
            if (userExists)
            {
                throw new ConflictException("User with this email already exists.", $"Email: {request.Dto.Email}");
            }

            _passwordHasher.CreatePasswordHash(request.Dto.Password, out var hash, out var salt);

            var user = new User
            {
                UserName = request.Dto.Username, 
                Email = request.Dto.Email,
                CustomPasswordHash = hash, 
                CustomPasswordSalt = salt 
            };

            await _userRepository.CreateUserAsync(user, cancellationToken);

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to assign 'User' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }

            return await _jwtService.GenerateToken(user); 
        }
    }
}
