using MediatR;

namespace RentCarX.Application.CQRS.Commands.Car.DeleteCar
{
    public class DeleteCarCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}
