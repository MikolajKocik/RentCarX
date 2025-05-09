using Microsoft.EntityFrameworkCore;
using RentCarX.Domain.Models; 
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Infrastructure.Data; 

namespace RentCarX.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly RentCarX_DbContext _context;

    public UserRepository(RentCarX_DbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }
}