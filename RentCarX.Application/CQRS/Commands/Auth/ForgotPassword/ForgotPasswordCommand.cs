using MediatR;
using RentCarX.Application.DTOs.Auth;

namespace RentCarX.Application.CQRS.Commands.Auth.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResponseDto>;
}
