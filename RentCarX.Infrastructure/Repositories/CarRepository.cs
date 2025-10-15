using Microsoft.EntityFrameworkCore;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;
using RentCarX.Infrastructure.Data;

namespace RentCarX.Infrastructure.Repositories;

public sealed class CarRepository : ICarRepository
{
    private readonly RentCarX_DbContext _context;

    public CarRepository(RentCarX_DbContext context)
    {
        _context = context;
    }

    public async Task UpdateAvailabilityForCarsAsync(IEnumerable<Guid> carIds, bool isAvailable, CancellationToken cancellationToken)
        => await _context.Cars
            .Where(c => carIds.Contains(c.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsAvailable, isAvailable), cancellationToken);

    public async Task CreateAsync(Car car, CancellationToken cancellation)
    {
        await _context.Cars.AddAsync(car, cancellation);
        await _context.SaveChangesAsync(cancellation);
    }

    public async Task<List<Car>> GetUnavailableCarsAsync(CancellationToken cancellationToken)
        => await _context.Cars
            .Where(c => !c.IsAvailable)
            .ToListAsync(cancellationToken);

    public async Task<Car?> GetCarByIdAsync(Guid id, CancellationToken cancellation) 
        => await _context.Cars 
        .FirstOrDefaultAsync(c => c.Id == id, cancellation);

    public async Task<ICollection<Car>> GetAllAsync(CancellationToken cancellation) 
        => await _context.Cars
        .ToListAsync(cancellation);

    public async Task RemoveAsync(Guid id, CancellationToken cancellation)
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

    public IQueryable<Car> GetFilteredCarsQuery(
         string? brand,
         string? model,
         string? fuelType,
         decimal? minPrice,
         decimal? maxPrice,
         bool? isAvailable)
    {
        var query = _context.Cars.AsQueryable();

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(c => c.Brand.Contains(brand));

        if (!string.IsNullOrWhiteSpace(model))
            query = query.Where(c => c.Model.Contains(model));

        if (!string.IsNullOrWhiteSpace(fuelType))
            query = query.Where(c => c.FuelType == fuelType);

        if (minPrice.HasValue)
            query = query.Where(c => c.PricePerDay >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(c => c.PricePerDay <= maxPrice.Value);

        if (isAvailable.HasValue)
            query = query.Where(c => c.IsAvailable == isAvailable.Value);

        return query; 
    }
}