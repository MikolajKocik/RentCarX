using MediatR;
using RentCarX.Application.DTOs;

namespace RentCarX.Application.CQRS.Commands.Car.EditCar
{
    public class EditCarCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public CarDto CarDto { get; set; } = default!;
    }
}