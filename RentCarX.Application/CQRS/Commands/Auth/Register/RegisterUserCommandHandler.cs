using Microsoft.AspNet.Identity;
namespace RentCarX.Application.CQRS.Commands.Auth.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
    {
        private readonly RentCarX_DbContext _context;
        private readonly JwtTokenService _jwtService;

        public RegisterUserCommandHandler(RentCarX_DbContext context, JwtTokenService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userExists = _context.Users.Any(u => u.Email == request.Dto.Email);
            if (userExists)
                throw new Exception("User already exists.");

            PasswordHasher.CreatePasswordHash(request.Dto.Password, out var hash, out var salt);

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
