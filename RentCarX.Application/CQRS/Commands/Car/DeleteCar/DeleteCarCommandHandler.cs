using MediatR;
using RentCarX.Application.CQRS.Commands.Car.DeleteCar;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

public class DeleteCarCommandHandler : IRequestHandler<DeleteCarCommand, Unit>
{
    private readonly ICarRepository _carRepository;

    public DeleteCarCommandHandler(ICarRepository carRepository)
    {
        _carRepository = carRepository;
    }

    public async Task<Unit> Handle(DeleteCarCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var entity = await _carRepository.GetCarByIdAsync(request.Id, cancellationToken);

        if (entity == null) throw new NotFoundException("Car", request.Id.ToString());


        await _carRepository.RemoveAsync(request.Id, cancellationToken);

        return Unit.Value;
    }
}
