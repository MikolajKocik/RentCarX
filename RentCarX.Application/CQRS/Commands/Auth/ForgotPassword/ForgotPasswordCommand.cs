using MediatR;
using RentCarX.Application.DTOs.Auth;

namespace RentCarX.Application.CQRS.Commands.Auth.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<ForgotPasswordResponseDto> 
    {
        public string Email { get; set; } 
    }
}
