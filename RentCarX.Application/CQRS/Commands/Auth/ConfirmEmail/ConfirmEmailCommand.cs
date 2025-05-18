using MediatR;

namespace RentCarX.Application.CQRS.Commands.Auth.ConfirmEmail
{
    public record ConfirmEmailCommand(Guid userId, string token) : IRequest<bool>;
}
