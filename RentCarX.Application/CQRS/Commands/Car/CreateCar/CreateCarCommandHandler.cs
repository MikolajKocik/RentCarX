using AutoMapper;
using MediatR;
using RentCarX.Application.CQRS.Commands.Car.AddCar;
using RentCarX.Domain.Interfaces.Repositories;
using Stripe;

namespace RentCarX.Application.CQRS.Commands.Car.CreateCar;

public class CreateCarCommandHandler : IRequestHandler<CreateCarCommand, Guid>
{
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;
    private readonly ProductService _productService;


    public CreateCarCommandHandler(
        ICarRepository carRepository,
        IMapper mapper,
        ProductService productService
        )
    {
        _carRepository = carRepository;
        _mapper = mapper;
        _productService = productService;
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
                Currency = "pln",
            },
        };

        // stripe product
        Product product = await _productService.CreateAsync(productOptions, cancellationToken: cancellationToken);

        var car = _mapper.Map<RentCarX.Domain.Models.Car>(request.CarData);
        car.Id = Guid.NewGuid();

        // Assign the new Stripe IDs to the car object
        car.StripeProductId = product.Id;
        car.StripePriceId = product.DefaultPriceId;  

        await _carRepository.CreateAsync(car, cancellationToken);

        return car.Id;
    }
}