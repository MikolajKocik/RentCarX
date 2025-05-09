using MediatR;
using Microsoft.EntityFrameworkCore;
using RentCarX.Application.DTOs.Car;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Car.GetDetails
{
    public class CarDetailsQueryHandler : IRequestHandler<CarDetailsQuery, CarDto?>
    {
        private readonly ICarRepository _carRepository; 

        public CarDetailsQueryHandler(ICarRepository carRepository) 
        {
            _carRepository = carRepository;
        }

        public async Task<CarDto?> Handle(CarDetailsQuery request, CancellationToken cancellationToken)
        {
            var car = await _carRepository.GetCarByIdAsync(request.Id, cancellationToken);

            if (car == null) return null;

            var carDto = new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                FuelType = car.FuelType,
                PricePerDay = car.PricePerDay,
                IsAvailable = car.IsAvailable
            };

            return carDto;
        }
    }
}
