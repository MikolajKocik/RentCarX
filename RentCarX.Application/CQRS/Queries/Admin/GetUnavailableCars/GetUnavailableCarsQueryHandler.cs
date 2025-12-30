using MediatR;
using Microsoft.Extensions.Logging;
using RentCarX.Application.DTOs.Car;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Admin.GetUnavailableCars;

public sealed class GetUnavailableCarsQueryHandler : IRequestHandler<GetUnavailableCarsQuery, List<CarStatusDto>>
{
    private readonly ICarRepository _carRepository;
    private readonly ILogger<GetUnavailableCarsQueryHandler> _logger;

    public GetUnavailableCarsQueryHandler(ICarRepository carRepository, ILogger<GetUnavailableCarsQueryHandler> logger)
    {
        _carRepository = carRepository;
        _logger = logger;
    }

    public async Task<List<CarStatusDto>> Handle(GetUnavailableCarsQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Models.Car> cars = await _carRepository.GetUnavailableCarsAsync(cancellationToken);
        _logger.LogInformation("Get unavailable cars query initialized");

        return cars.Select(c => new CarStatusDto
        {
            Id = c.Id,
            Name = $"{c.Brand} {c.Model}",
            IsAvailable = c.IsAvailable,
            ActiveReservations = c.Reservations.Select(r => new ReservationBriefDto
            {
                Id = r.Id,
                StartDate = r.StartDate,
                EndDate = r.EndDate
            }).ToList()
        }).ToList();
    }
}
