using MediatR;
using RentCarX.Application.DTOs.Auth;

namespace RentCarX.Application.CQRS.Commands.Auth.Register
{
    public record RegisterUserCommand(RegisterUserDto Dto) : IRequest<string>;

}
