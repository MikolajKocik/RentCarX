using MediatR;

namespace RentCarX.Application.CQRS.Commands.Auth.ResetPassword
{
    public record ResetPasswordCommand(Guid UserId, string Token, string NewPassword) : IRequest<bool>;

}
