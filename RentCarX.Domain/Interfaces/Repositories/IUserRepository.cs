namespace RentCarX.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsersForAdminAsync(CancellationToken ct);
}
