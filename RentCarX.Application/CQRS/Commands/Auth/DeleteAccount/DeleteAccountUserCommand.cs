using MediatR;
using RentCarX.Application.DTOs.Auth;

namespace RentCarX.Application.CQRS.Commands.Auth.DeleteAccount;

public sealed record DeleteAccountUserCommand(DeleteAccountDto Dto) : IRequest<Unit>;
