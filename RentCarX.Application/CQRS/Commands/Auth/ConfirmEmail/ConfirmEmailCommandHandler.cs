using MediatR;
using Microsoft.AspNetCore.Identity;
using RentCarX.Domain.Exceptions;

namespace RentCarX.Application.CQRS.Commands.Auth.ConfirmEmail
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
    {
        private readonly UserManager<User> _userManager;

        public ConfirmEmailCommandHandler(UserManager<User> userManager) => _userManager = userManager;

        public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.userId.ToString());
            if (user == null) throw new NotFoundException(nameof(user), "User not found.");

            var result = await _userManager.ConfirmEmailAsync(user, request.token);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"Confirm email failed for user {request.userId}: {errors}"); 
            }

            return result.Succeeded;
        }
    }
}
