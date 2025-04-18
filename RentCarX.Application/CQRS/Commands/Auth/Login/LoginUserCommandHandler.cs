using MediatR;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Infrastructure.Services;

namespace RentCarX.Application.CQRS.Commands.Auth.Login
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, string>
    {
        private readonly IRentCarX_DbContext _context;
        private readonly JwtTokenService _jwtService;

        public LoginUserHandler(IRentCarX_DbContext context, JwtTokenService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Dto.Email, cancellationToken);

            if (user is null || !PasswordHasher.VerifyPasswordHash(request.Dto.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            return _jwtService.GenerateToken(user);
        }
    }

}
