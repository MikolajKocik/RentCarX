using MediatR;
using RentCarX.Application.DTOs.Auth;

namespace RentCarX.Application.CQRS.Commands.Auth.Register
{
    public class RegisterUserCommand: IRequest<RegisterUserResponseDto>
    {
        public RegisterUserDto Dto { get; } 

        public RegisterUserCommand(RegisterUserDto dto) 
        {
            Dto = dto;
        }
    }

}
