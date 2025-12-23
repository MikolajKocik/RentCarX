using Microsoft.EntityFrameworkCore;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Infrastructure.Data;

namespace RentCarX.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly RentCarX_DbContext _context;

    public UserRepository(RentCarX_DbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllUsersForAdminAsync(CancellationToken ct)
        => await _context.Users
            .IgnoreQueryFilters()
            .OrderByDescending(u => u.DeletedAt) 
            .ToListAsync(ct);
}
