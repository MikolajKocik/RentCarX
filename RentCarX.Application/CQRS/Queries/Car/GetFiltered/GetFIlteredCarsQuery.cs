using MediatR;
using RentCarX.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Queries.Car.GetFiltered
{
    public record GetFilteredCarsQuery(
    string? Brand,
    string? Model,
    string? FuelType,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? IsAvailable
    ) : IRequest<List<CarDto>>;

}
