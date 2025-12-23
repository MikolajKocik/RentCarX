using MediatR;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Admin.GetDeletedUsers;

public sealed class GetDeletedUsersQueryHandler : IRequestHandler<GetDeletedUsersQuery, IEnumerable<User>>
{
    private readonly IUserRepository _userRepository;

    public GetDeletedUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<User>> Handle(GetDeletedUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllUsersForAdminAsync(cancellationToken);
        return users.Any() ? users : Enumerable.Empty<User>();
    }
}