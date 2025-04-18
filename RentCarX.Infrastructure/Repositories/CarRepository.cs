using Microsoft.EntityFrameworkCore;
using RentCarX.Domain.Entities;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Infrastructure.Data;

namespace RentCarX.Infrastructure.Repositories;

public class CarRepository : ICarRepository
{
    private readonly RentCarX_DbContext _context;

    public CarRepository(RentCarX_DbContext context)
    {
        _context = context;
    }
    public async Task CreateAsync(Car car)
    {
        await _context.Cars.AddAsync(car);
        await _context.SaveChangesAsync();
    }

    public async Task<Car?> GetCarByIdAsync(int id, CancellationToken cancellation)
        => await _context.Cars
        .Include(c => c.Images)
        .FirstOrDefaultAsync(c => c.Id == id, cancellation);

    public async Task<ICollection<Car>> GetAllAsync(CancellationToken cancellation)
        => await _context.Cars
        .Include(c => c.Images)
        .ToListAsync(cancellation);

    public async Task CommitAsync() => await _context.SaveChangesAsync();


    public async Task RemoveAsync(int id, CancellationToken cancellation)
    {
        var carToRemove = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id, cancellation);

        _context.Cars.Remove(carToRemove!);

        await _context.SaveChangesAsync(cancellation);
    }

    public async Task UpdateCarAsync(Car car, CancellationToken cancellation)
    {
        _context.Cars.Update(car);
        await _context.SaveChangesAsync(cancellation);
    }
}