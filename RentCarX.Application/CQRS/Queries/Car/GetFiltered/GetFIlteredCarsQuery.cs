using MediatR;
using RentCarX.Application.DTOs.Car;

namespace RentCarX.Application.CQRS.Queries.Car.GetFiltered;

public record GetFilteredCarsQuery(
    string? Brand,
    string? Model,
    string? FuelType,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? Year,
    bool? IsAvailable,
    int PageNumber = 1,
    int PageSize = 10
    ) : IRequest<List<CarDto>>;
