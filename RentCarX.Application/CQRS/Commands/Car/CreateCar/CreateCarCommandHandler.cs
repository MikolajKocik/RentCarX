using MediatR;
using RentCarX.Domain.Interfaces.Repositories; 

using RentCarX.Application.CQRS.Commands.Car.AddCar;

namespace RentCarX.Application.CQRS.Commands.Car.CreateCar
{
    public class CreateCarCommandHandler : IRequestHandler<CreateCarCommand, Guid>
    {
        private readonly ICarRepository _carRepository;

        public CreateCarCommandHandler(ICarRepository carRepository) 
        {
            _carRepository = carRepository;
        }

        public async Task<Guid> Handle(CreateCarCommand request, CancellationToken cancellationToken)
        {
            var car = new Car 
            {
                Id = Guid.NewGuid(),
                Brand = request.CarDto.Brand,
                Model = request.CarDto.Model,
                FuelType = request.CarDto.FuelType,
                PricePerDay = request.CarDto.PricePerDay,
                Year = request.CarDto.Year,
                IsAvailable = request.CarDto.IsAvailable 
            };

            await _carRepository.CreateAsync(car); 

            return car.Id;
        }
    }
}