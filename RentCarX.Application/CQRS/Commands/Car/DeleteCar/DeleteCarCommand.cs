using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Commands.Car.DeleteCar
{
    public class DeleteCarCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}
