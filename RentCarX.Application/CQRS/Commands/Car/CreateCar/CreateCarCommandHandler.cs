using MediatR;
using RentCarX.Application.CQRS.Commands.Car.AddCar;
using RentCarX.Domain.Interfaces.DbContext;

namespace RentCarX.Application.CQRS.Commands.Car.CreateCar
{
    public class CreateCarCommandHandler : IRequestHandler<CreateCarCommand, Guid>
    {
        private readonly IRentCarX_DbContext _context;

        public CreateCarCommandHandler(IRentCarX_DbContext context)
        {
            _context = context;
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

            _context.Cars.Add(car);
            await _context.SaveChangesAsync(cancellationToken);

            return car.Id;
        }
    }

}
