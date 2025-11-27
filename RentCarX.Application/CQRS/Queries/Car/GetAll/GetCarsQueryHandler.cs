using MediatR;
using Microsoft.EntityFrameworkCore;
using RentCarX.Application.DTOs.Car;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Car.GetAll
{
    public class GetCarsQueryHandler : IRequestHandler<GetCarsQuery, List<CarDto>>
    {
        private readonly ICarRepository _carRepository; 

        public GetCarsQueryHandler(ICarRepository carRepository) 
        {
            _carRepository = carRepository;
        }

        public async Task<List<CarDto>> Handle(GetCarsQuery request, CancellationToken cancellationToken)
        {
            var query = (await _carRepository.GetAllAsync(cancellationToken)).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Brand))
            {
                query = query.Where(c => c.Brand.ToLower().Contains(request.Brand.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.FuelType))
            {
                query = query.Where(c => c.FuelType.ToLower() == request.FuelType.ToLower());
            }

            var result = await query
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    Model = c.Model,
                    PricePerDay = c.PricePerDay,
                    Year = c.Year, 
                    FuelType = c.FuelType
                })
                .ToListAsync(cancellationToken);

            return result;
        }
    }

}
