using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Queries.Car.GetAll
{
    public record GetCarsQuery(string? Brand, string? FuelType) : IRequest<List<CarDto>>;

}
