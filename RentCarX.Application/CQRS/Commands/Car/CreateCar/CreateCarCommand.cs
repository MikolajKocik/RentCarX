using MediatR;
using RentCarX.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Commands.Car.AddCar
{
    public class CreateCarCommand : IRequest<Guid>
    {
        public CarDto CarDto { get; set; } = default!;
    }

}
