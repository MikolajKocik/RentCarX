using MediatR;
using RentCarX.Application.DTOs.Car;

namespace RentCarX.Application.CQRS.Commands.Car.EditCar
{
    public class EditCarCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public EditCarDto CarData { get; set; } = default!; 
    }
}