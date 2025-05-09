using AutoMapper;
using MediatR;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;


namespace RentCarX.Application.CQRS.Commands.Car.EditCar
{
    public class EditCarCommandHandler : IRequestHandler<EditCarCommand, Unit>
    {
        private readonly ICarRepository _carRepository;
        private readonly IMapper _mapper;

        public EditCarCommandHandler(ICarRepository carRepository, IMapper mapper)
        {
            _carRepository = carRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(EditCarCommand request, CancellationToken cancellationToken)
        {
            var car = await _carRepository.GetCarByIdAsync(request.Id, cancellationToken);

            if (car == null)
            {
                throw new NotFoundException("Car", request.Id.ToString()); 
            }

            _mapper.Map(request.CarData, car);

            await _carRepository.UpdateCarAsync(car, cancellationToken);

            return Unit.Value;
        }
    }
}