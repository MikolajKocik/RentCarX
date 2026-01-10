using RentCarX.Domain.Models;

namespace RentCarX.Domain.Interfaces.Repositories
{
    public interface ICarRepository
    {
        Task CreateAsync(Car car, CancellationToken cancellationToken);
        Task<ICollection<Car>> GetAllAsync(CancellationToken cancellation);
        Task<Car?> GetCarByIdAsync(Guid id, CancellationToken cancellation);
        Task UpdateCarAsync(Car car, CancellationToken cancellation);
        Task RemoveAsync(Guid id, CancellationToken cancellation);
        IQueryable<Car> GetFilteredCarsQuery(
         string? brand,
         string? model,
         string? fuelType,
         decimal? minPrice,
         decimal? maxPrice,
         int? year,
         bool? isAvailable);
        Task UpdateAvailabilityForCarsAsync(IEnumerable<Guid> carIds, bool isAvailable, CancellationToken cancellationToken);
        Task<List<Car>> GetUnavailableCarsAsync(CancellationToken cancellationToken);
    }
}
