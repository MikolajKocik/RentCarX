using MediatR;
using RentCarX.Application.DTOs.Auth;

namespace RentCarX.Application.CQRS.Commands.Auth.Login
{
    public record LoginUserCommand(LoginDto Dto) : IRequest<string>;

}
