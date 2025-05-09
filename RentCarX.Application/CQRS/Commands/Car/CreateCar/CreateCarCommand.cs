using MediatR;
using RentCarX.Application.DTOs;

namespace RentCarX.Application.CQRS.Commands.Car.AddCar
{
    public class CreateCarCommand : IRequest<Guid>
    {
        public CarDto CarDto { get; set; } = default!;
    }

}
