using MediatR;
using RentCarX.Domain.Interfaces.Repositories; 

using RentCarX.Application.CQRS.Commands.Car.AddCar;
using AutoMapper;

namespace RentCarX.Application.CQRS.Commands.Car.CreateCar
{
    public class CreateCarCommandHandler : IRequestHandler<CreateCarCommand, Guid>
    {
        private readonly ICarRepository _carRepository;
        private readonly IMapper _mapper;

        public CreateCarCommandHandler(ICarRepository carRepository, IMapper mapper)
        {
            _carRepository = carRepository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateCarCommand request, CancellationToken cancellationToken)
        {
            var car = _mapper.Map<RentCarX.Domain.Models.Car>(request.CarData);
            car.Id = Guid.NewGuid();

            await _carRepository.CreateAsync(car, cancellationToken);

            return car.Id;
        }
    }
}