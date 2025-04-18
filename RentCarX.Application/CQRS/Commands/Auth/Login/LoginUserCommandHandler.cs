using MediatR;
using Microsoft.EntityFrameworkCore;
using RentCarX.Application.Interfaces.JWT;
using RentCarX.Application.Interfaces.PasswordHasher;
using RentCarX.Domain.Interfaces.DbContext;

namespace RentCarX.Application.CQRS.Commands.Auth.Login
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, string>
    {
        private readonly IRentCarX_DbContext _context;
        private readonly IJwtTokenService _jwtService;
        private readonly IPasswordHasher _passwordHasher;

        public LoginUserHandler(IRentCarX_DbContext context, IJwtTokenService jwtService, IPasswordHasher passwordHasher)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Dto.Email, cancellationToken);

            if (user is null || !_passwordHasher.VerifyPasswordHash(request.Dto.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            return _jwtService.GenerateToken(user);
        }
    }

}
