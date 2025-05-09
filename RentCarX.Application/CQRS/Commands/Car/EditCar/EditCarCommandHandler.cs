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

            car.Brand = request.CarData.Brand;
            car.Model = request.CarData.Model;
            car.FuelType = request.CarData.FuelType;
            car.PricePerDay = request.CarData.PricePerDay;
            car.Year = request.CarData.Year;
            car.IsAvailable = request.CarData.IsAvailable;

            await _carRepository.UpdateCarAsync(car, cancellationToken); 

            return Unit.Value;
        }
    }
}