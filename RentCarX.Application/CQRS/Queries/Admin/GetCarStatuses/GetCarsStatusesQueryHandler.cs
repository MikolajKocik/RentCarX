using AutoMapper;
using MediatR;
using RentCarX.Application.DTOs.Car;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models.Enums;

namespace RentCarX.Application.CQRS.Queries.Admin.GetCarStatuses;

public sealed class GetCarsStatusesQueryHandler : IRequestHandler<GetCarsStatusesQuery, ICollection<CarStatusDto>>
{
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;

    public GetCarsStatusesQueryHandler(ICarRepository carRepository, IMapper mapper)
    {
        _carRepository = carRepository;
        _mapper = mapper;
    }

    public async Task<ICollection<CarStatusDto>> Handle(GetCarsStatusesQuery request, CancellationToken cancellationToken)
    {
        var cars = await _carRepository.GetAllAsync(cancellationToken);

        return cars.Select(c => new CarStatusDto
        { 
            Id = c.Id,
            IsAvailable = c.IsAvailable,
            Name = $"{c.Brand} {c.Model}",
            ActiveReservations = _mapper.Map<List<ReservationBriefDto>>(
                c.Reservations
                    .Where(r => r.Status != ReservationStatus.Cancelled &&
                        !r.IsDeleted)
                    .ToList())
        }).ToList();
    }
}
