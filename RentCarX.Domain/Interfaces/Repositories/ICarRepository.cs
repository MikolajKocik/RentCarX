using RentCarX.Domain.Models;

namespace RentCarX.Domain.Interfaces.Repositories
{
    public interface ICarRepository
    {
        Task CreateAsync(Car car);

        Task<ICollection<Car>> GetAllAsync(CancellationToken cancellation);

        Task<Car?> GetCarByIdAsync(int id, CancellationToken cancellation);

        Task CommitAsync();
        Task UpdateCarAsync(Car car, CancellationToken cancellation);

        Task RemoveAsync(int id, CancellationToken cancellation);

    }
}
