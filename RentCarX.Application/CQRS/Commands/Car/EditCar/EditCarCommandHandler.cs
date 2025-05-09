using MediatR;
using RentCarX.Domain.Interfaces.Repositories;


namespace RentCarX.Application.CQRS.Commands.Car.EditCar
{
    public class EditCarCommandHandler : IRequestHandler<EditCarCommand, Unit>
    {
        private readonly ICarRepository _carRepository;

        public EditCarCommandHandler(ICarRepository carRepository) 
        {
            _carRepository = carRepository;
        }

        public async Task<Unit> Handle(EditCarCommand request, CancellationToken cancellationToken)
        {
            var car = await _carRepository.GetCarByIdAsync(request.Id, cancellationToken);

            if (car == null) return Unit.Value;

            car.Brand = request.CarDto.Brand;
            car.Model = request.CarDto.Model;
            car.FuelType = request.CarDto.FuelType;
            car.PricePerDay = request.CarDto.PricePerDay;
            car.Year = request.CarDto.Year;
            car.IsAvailable = request.CarDto.IsAvailable;

            await _carRepository.UpdateCarAsync(car, cancellationToken); 

            return Unit.Value;
        }
    }
}