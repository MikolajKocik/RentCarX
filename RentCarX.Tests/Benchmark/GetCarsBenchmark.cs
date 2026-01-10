using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentCarX.Application.CQRS.Queries.Car.GetFiltered;
using RentCarX.Application.MappingsProfile;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Models;
using RentCarX.Infrastructure.Data;
using RentCarX.Infrastructure.Repositories;
using System.ComponentModel;

namespace RentCarX.Tests.Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 3, iterationCount: 10)]
[HtmlExporter, CsvExporter, RPlotExporter]
[Description("Benchmark for cars loading")]
public class GetCarsBenchmark
{
    private IMediator mediator;
    private IServiceScope serviceScope;

    [Params(100, 1000, 10000)]
    public int NumberOfCars;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddDbContext<RentCarX_DbContext>(options =>
            options.UseInMemoryDatabase("BenchmarkDb"));

        // Rejestracja repozytoriów i innych serwisów
        services.AddScoped<ICarRepository, CarRepository>();

        // Rejestracja AutoMapper
        services.AddAutoMapper(typeof(CarMappingProfile));

        // Rejestracja MediatR i handlerów z odpowiedniego assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetFilteredCarsQuery).Assembly));

        var serviceProvider = services.BuildServiceProvider();
        serviceScope = serviceProvider.CreateScope();
        mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();
        var context = serviceScope.ServiceProvider.GetRequiredService<RentCarX_DbContext>();

        SeedDatabase(context);
    }
    private void SeedDatabase(RentCarX_DbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var random = new Random();
        var fuelTypes = new[] { "LPG", "Diesel", "Electric", "Hybrid", "Petrol" };
        var cars = new List<Car>();

        for (int i = 0; i < NumberOfCars; i++)
        {
            cars.Add(new Car
            {
                Id = Guid.NewGuid(),
                Brand = "Brand",
                Model = "",
                FuelType = fuelTypes[random.Next(fuelTypes.Length)],
                PricePerDay = random.Next(50, 500),
                Year = random.Next(2010, 2024),
                IsAvailable = true,
            });
        }
        context.Cars.AddRange(cars);
        context.SaveChanges();
    }

    [GlobalCleanup]
    public void Cleanup() 
    { 
        serviceScope?.Dispose();
    }

    [Benchmark(Description = "Get all cars")]
    public async Task GetAllCars()
    {
        await mediator.Send(new GetFilteredCarsQuery(null, null, null ,null ,null, null, true));
    }

    [Benchmark(Description = "Get filtered cars by fuel type")]
    public async Task GetCarsByFuel()
    {
        await mediator.Send(new GetFilteredCarsQuery(null, null, "Diesel", null, null, null, true));
    }

    [Benchmark(Description = "Get filtered cars by price")]
    public async Task GetCarsByPriceRange()
    {
        await mediator.Send(new GetFilteredCarsQuery(null, null, null, 50, 100, null, true));
    }

    [Benchmark(Description = "Get filtered cars by production year")]
    public async Task GetCarsByProductionYear()
    {
        await mediator.Send(new GetFilteredCarsQuery(null, null ,null ,null ,null, 2020, true));
    }
}
