using MediatR;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Commands.Car.DeleteCar
{
    public class DeleteCarCommandHandler : IRequestHandler<DeleteCarCommand>
    {
        private readonly ICarRepository _carRepository;

        public DeleteCarCommandHandler(ICarRepository carRepository) 
        {
            _carRepository = carRepository;
        }

        public async Task<Unit> Handle(DeleteCarCommand request, CancellationToken cancellationToken)
        {
            var entity = await _carRepository.GetCarByIdAsync(request.Id, cancellationToken);
            if (entity == null) return Unit.Value;

            await _carRepository.RemoveAsync(request.Id, cancellationToken); 

            return Unit.Value;
        }
    }
}
