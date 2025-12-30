using MediatR;
using RentCarX.Application.DTOs.Car;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Admin.GetCarStatuses;

public sealed class GetCarsStatusesQueryHandler : IRequestHandler<GetCarsStatusesQuery, ICollection<CarStatusDto>>
{
    private readonly ICarRepository _carRepository;

    public GetCarsStatusesQueryHandler(ICarRepository carRepository)
    {
        _carRepository = carRepository;
    }

    public async Task<ICollection<CarStatusDto>> Handle(GetCarsStatusesQuery request, CancellationToken cancellationToken)
    {
        var cars = await _carRepository.GetAllAsync(cancellationToken);

        return cars.Select(c => new CarStatusDto
        { 
            Id = c.Id,
            IsAvailable = c.IsAvailable,
            Name = $"{c.Brand} {c.Model}"
        }).ToList();
    }
}
