using MediatR;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Application.Interfaces.PasswordHasher;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Models;
namespace RentCarX.Application.CQRS.Commands.Auth.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
    {
        private readonly IRentCarX_DbContext _context;
        private readonly IJwtTokenService _jwtService;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserCommandHandler(IRentCarX_DbContext context, IJwtTokenService jwtService, IPasswordHasher passwordHasher)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userExists = _context.Users.Any(u => u.Email == request.Dto.Email);
            if (userExists)
                throw new Exception("User already exists.");

            _passwordHasher.CreatePasswordHash(request.Dto.Password, out var hash, out var salt);

            var user = new User
            {
                Username = request.Dto.Username,
                Email = request.Dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return _jwtService.GenerateToken(user);
        }
    }
}
