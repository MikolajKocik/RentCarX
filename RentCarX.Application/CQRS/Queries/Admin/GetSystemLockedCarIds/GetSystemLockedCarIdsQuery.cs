using MediatR;

namespace RentCarX.Application.CQRS.Queries.Admin.GetSystemLockedCarIds;

public sealed record GetSystemLockedCarIdsQuery() : IRequest<List<Guid>>;