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

        Task<ICollection<Car>> GetFilteredCarsAsync(
            string? brand,
            string? model,
            string? fuelType,
            decimal? minPrice,
            decimal? maxPrice,
            bool? isAvailable,
            CancellationToken cancellationToken);
    }
}
