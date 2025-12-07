using RentCarX.Application.DTOs.Car;
using RentCarX.Application.Interfaces.Services.Reports;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;
using System.Collections.Concurrent;

namespace RentCarX.Application.Helpers;

public sealed class ReportHelper : IReportingConfiguration
{
    private readonly ICarRepository _carRepository;

    public ReportHelper(ICarRepository repository)
    {
        _carRepository = repository;
    }

    public async Task<ConcurrentBag<ReportCarDto>> SetReport(CancellationToken ct)
    {
        ICollection<Car> cars = await _carRepository.GetAllAsync(ct);

        var reportData = new ConcurrentBag<ReportCarDto>();

        cars.AsParallel().ForAll(car =>
        {
            var dto = new ReportCarDto
            {
                CarId = car.Id,
                Model = car.Model,
                PricePerDay = car.PricePerDay,
                TotalReservations = car.Reservations.Count,
                Revenue = car.Reservations.Sum(r => r.TotalCost)
            };

            reportData.Add(dto);
        });

        return reportData;
    }
}
