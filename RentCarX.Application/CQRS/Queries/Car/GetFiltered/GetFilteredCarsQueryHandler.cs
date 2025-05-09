using MediatR;
using RentCarX.Application.DTOs.Car;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Car.GetFiltered
{
    public class GetFilteredCarsQueryHandler : IRequestHandler<GetFilteredCarsQuery, List<CarDto>>
    {
        private readonly ICarRepository _carRepository; 

        public GetFilteredCarsQueryHandler(ICarRepository carRepository) 
        {
            _carRepository = carRepository;
        }

        public async Task<List<CarDto>> Handle(GetFilteredCarsQuery request, CancellationToken cancellationToken)
        { 
            var cars = await _carRepository.GetFilteredCarsAsync(
                request.Brand,
                request.Model,
                request.FuelType,
                request.MinPrice,
                request.MaxPrice,
                request.IsAvailable,
                cancellationToken);

            return cars.Select(c => new CarDto
            {
                Id = c.Id,
                Brand = c.Brand,
                Model = c.Model,
                Year = c.Year,
                FuelType = c.FuelType,
                PricePerDay = c.PricePerDay,
                IsAvailable = c.IsAvailable
            })
                .ToList();
        }
    }
}
