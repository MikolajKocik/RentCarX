using MediatR;
using RentCarX.Application.DTOs.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Queries.Car.GetDetails
{
    public record CarDetailsQuery(Guid Id) : IRequest<CarDto>;

}
