using MediatR;
using RentCarX.Application.DTOs;

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
