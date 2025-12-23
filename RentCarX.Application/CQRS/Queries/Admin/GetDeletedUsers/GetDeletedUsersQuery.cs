using MediatR;

namespace RentCarX.Application.CQRS.Queries.Admin.GetDeletedUsers;

public sealed record GetDeletedUsersQuery() : IRequest<IEnumerable<User>>;

