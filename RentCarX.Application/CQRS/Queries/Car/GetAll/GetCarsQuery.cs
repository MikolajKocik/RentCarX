using MediatR;
using RentCarX.Application.DTOs;

namespace RentCarX.Application.CQRS.Queries.Car.GetAll
{
    public record GetCarsQuery(string? Brand, string? FuelType) : IRequest<List<CarDto>>;

}
