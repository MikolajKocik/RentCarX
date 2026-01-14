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
    private readonly IMapper _mapper;
    private readonly ProductService _productService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<CreateCarCommandHandler> _logger;

    public CreateCarCommandHandler(
        ICarRepository carRepository,
        IMapper mapper,
        ProductService productService,
         IFileUploadService fileUploadService,
        ILogger<CreateCarCommandHandler> logger
        )
    {
        _carRepository = carRepository;
        _mapper = mapper;
        _productService = productService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateCarCommand request, CancellationToken cancellationToken)
    {
        var productOptions = new ProductCreateOptions
        {
            Name = $"{request.CarData.Brand} {request.CarData.Model} ({request.CarData.Year})",
            Description = $"Rental for {request.CarData.Brand} {request.CarData.Model}",
            DefaultPriceData = new ProductDefaultPriceDataOptions
            {
                UnitAmountDecimal = request.CarData.PricePerDay * 100,
                Currency = "usd",
            },
        };

        // stripe product
        Product product = await _productService.CreateAsync(productOptions, cancellationToken: cancellationToken);

        var car = _mapper.Map<RentCarX.Domain.Models.Car>(request.CarData);
        car.Id = Guid.NewGuid();

        // Assign the new Stripe IDs to the car object
        car.StripeProductId = product.Id;
        car.StripePriceId = product.DefaultPriceId;

        if (request.CarData.Image != null)
        {
            try
            {
                car.ImageUrl = await _fileUploadService.UploadImageAsync(request.CarData.Image, cancellationToken);
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