using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.Services.Stripe;
using Stripe;

namespace RentCarX.Infrastructure.Services.Stripe
{
    public class StripeProductService : IStripeProductService
    {
        private readonly ICarRepository _carRepository;
        private readonly PriceService _priceService;
        private readonly ProductService _productService;

        public StripeProductService(
            ICarRepository carRepository,
            PriceService priceService,
            ProductService productService
            )
        {
            _carRepository = carRepository;
            _priceService = priceService;
            _productService = productService;
        }

        public async Task SyncProductsFromCarsAsync(CancellationToken cancellationToken = default)
        {
            var cars = await _carRepository.GetAllAsync(cancellationToken);

            foreach (var car in cars)
            {
                if (!string.IsNullOrEmpty(car.StripeProductId) && !string.IsNullOrEmpty(car.StripePriceId))
                    continue; 

                // Create Stripe Product
                var product = await _productService.CreateAsync(new ProductCreateOptions
                {
                    Name = $"{car.Brand} {car.Model}",
                    Description = $"Rental car - {car.FuelType}, {car.Year} model year",
                }, cancellationToken: cancellationToken);

                // Create Stripe Price
                var price = await _priceService.CreateAsync(new PriceCreateOptions
                {
                    UnitAmountDecimal = car.PricePerDay * 100, 
                    Currency = "usd",
                    Product = product.Id,
                }, cancellationToken: cancellationToken);

                car.StripeProductId = product.Id;
                car.StripePriceId = price.Id;

                await _carRepository.UpdateCarAsync(car, cancellationToken);
            }
        }
    }
}
