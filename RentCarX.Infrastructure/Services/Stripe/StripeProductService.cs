using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.Services.Stripe;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Infrastructure.Services.Stripe
{
    public class StripeProductService : IStripeProductService
    {
        private readonly ICarRepository _carRepository;

        public StripeProductService(ICarRepository carRepository)
        {
            _carRepository = carRepository;
        }

        public async Task SyncProductsFromCarsAsync(CancellationToken cancellationToken = default)
        {
            var cars = await _carRepository.GetAllAsync(cancellationToken);

            var productService = new ProductService();
            var priceService = new PriceService();

            foreach (var car in cars)
            {
                if (!string.IsNullOrEmpty(car.StripeProductId) && !string.IsNullOrEmpty(car.StripePriceId))
                    continue; // Już istnieje

                // Create Stripe Product
                var product = await productService.CreateAsync(new ProductCreateOptions
                {
                    Name = $"{car.Brand} {car.Model}",
                    Description = $"Rental car - {car.FuelType}, {car.Year} model year",
                }, cancellationToken: cancellationToken);

                // Create Stripe Price
                var price = await priceService.CreateAsync(new PriceCreateOptions
                {
                    UnitAmountDecimal = car.PricePerDay * 100, // grosze
                    Currency = "pln",
                    Product = product.Id,
                }, cancellationToken: cancellationToken);

                car.StripeProductId = product.Id;
                car.StripePriceId = price.Id;

                await _carRepository.UpdateCarAsync(car, cancellationToken);
            }
        }
    }
}
