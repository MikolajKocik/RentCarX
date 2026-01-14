using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RentCarX.Application.CQRS.Commands.Car.AddCar;
using RentCarX.Application.Interfaces.Services.File;
using RentCarX.Domain.Interfaces.Repositories;
using Stripe;

namespace RentCarX.Application.CQRS.Commands.Car.CreateCar;

public class CreateCarCommandHandler : IRequestHandler<CreateCarCommand, Guid>
{
    private readonly ICarRepository _carRepository;
    private readonly ProductService _productService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<CreateCarCommandHandler> _logger;

    public CreateCarCommandHandler(
        ICarRepository carRepository,
        ProductService productService,
         IFileUploadService fileUploadService,
        ILogger<CreateCarCommandHandler> logger
        )
    {
        _carRepository = carRepository;
        _productService = productService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateCarCommand request, CancellationToken cancellationToken)
    {
        var productOptions = new ProductCreateOptions
        {
            Name = $"{request.Brand} {request.Model} ({request.Year})",
            Description = $"Rental for {request.Brand} {request.Model}",
            DefaultPriceData = new ProductDefaultPriceDataOptions
            {
                UnitAmountDecimal = request.PricePerDay * 100,
                Currency = "usd",
            },
        };

        // stripe product
        Product product = await _productService.CreateAsync(productOptions, cancellationToken: cancellationToken);

        var car = new RentCarX.Domain.Models.Car
        {
            Id = Guid.NewGuid(),
            Brand = request.Brand,
            Model = request.Model,
            Year = request.Year,
            FuelType = request.FuelType,
            PricePerDay = request.PricePerDay,
            IsAvailable = request.IsAvailable,
            StripeProductId = product.Id,
            StripePriceId = product.DefaultPriceId
        };       

        if (request.Image != null)
        {
            try
            {
                car.ImageUrl = await _fileUploadService.UploadImageAsync(request.Image, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image for car");
                throw;
            }
        }

        await _carRepository.CreateAsync(car, cancellationToken);

        return car.Id;
    }
}