using MediatR;
using RentCarX.Application.DTOs.Car;

namespace RentCarX.Application.CQRS.Commands.Car.AddCar
{
    public class CreateCarCommand : IRequest<Guid>
    {
        public CreateCarDto CarData { get; set; } = default!; 
    }
}
