using MediatR;
using Microsoft.AspNetCore.Identity;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.UserContext;

namespace RentCarX.Application.CQRS.Commands.Auth.DeleteAccount;

public sealed class DeleteAccountUserCommandHandler : IRequestHandler<DeleteAccountUserCommand, Unit>
{
    private readonly IUserContextService _userContextService;
    private readonly UserManager<User> _userManager;

    public DeleteAccountUserCommandHandler(IUserContextService userContextService, UserManager<User> userManager)
    {
        _userContextService = userContextService;
        _userManager = userManager;
    }

    public async Task<Unit> Handle(DeleteAccountUserCommand request, CancellationToken cancellationToken)
    {
        Guid userId = _userContextService.UserId;
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new ArgumentNullException("User not found");

        bool isPasswordCorrect = await _userManager.CheckPasswordAsync(user, request.Dto.Password);
        if (!isPasswordCorrect)        
            throw new BadRequestException("Invalid password.");       

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new Exception("Failed to delete user account.");
        

        return Unit.Value;
    }
}
