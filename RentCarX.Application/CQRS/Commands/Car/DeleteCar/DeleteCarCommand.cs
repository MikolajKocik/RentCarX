using MediatR;
using System;

namespace RentCarX.Application.CQRS.Commands.Car.DeleteCar
{
    public class DeleteCarCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
    }
}
