using MediatR;

namespace RentCarX.Application.CQRS.Commands.Auth.Logout;

public sealed record LogoutCommand() : IRequest;
