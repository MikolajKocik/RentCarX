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
            var car = new RentCarX.Domain.Models.Car 
            {
                Id = Guid.NewGuid(),
                Brand = request.CarData.Brand,
                Model = request.CarData.Model,
                FuelType = request.CarData.FuelType,
                PricePerDay = request.CarData.PricePerDay,
                Year = request.CarData.Year,
                IsAvailable = request.CarData.IsAvailable 
            };

            await _carRepository.CreateAsync(car); 

            return car.Id;
        }
    }
}