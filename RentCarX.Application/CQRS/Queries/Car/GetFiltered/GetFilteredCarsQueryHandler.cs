using MediatR;
using Microsoft.EntityFrameworkCore;
using RentCarX.Application.DTOs;
using RentCarX.Domain.Interfaces.DbContext;

namespace RentCarX.Application.CQRS.Queries.Car.GetFiltered
{
    public class GetFilteredCarsQueryHandler : IRequestHandler<GetFilteredCarsQuery, List<CarDto>>
    {
        private readonly IRentCarX_DbContext _context;

        public GetFilteredCarsQueryHandler(IRentCarX_DbContext context)
        {
            _context = context;
        }

        public async Task<List<CarDto>> Handle(GetFilteredCarsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Cars.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Brand))
                query = query.Where(c => c.Brand.Contains(request.Brand));

            if (!string.IsNullOrWhiteSpace(request.Model))
                query = query.Where(c => c.Model.Contains(request.Model));

            if (!string.IsNullOrWhiteSpace(request.FuelType))
                query = query.Where(c => c.FuelType == request.FuelType);

            if (request.MinPrice.HasValue)
                query = query.Where(c => c.PricePerDay >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(c => c.PricePerDay <= request.MaxPrice.Value);

            if (request.IsAvailable.HasValue)
                query = query.Where(c => c.IsAvailable == request.IsAvailable.Value);

            return await query
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    Model = c.Model,
                    PricePerDay = c.PricePerDay,
                    IsAvailable = c.IsAvailable
                })
                .ToListAsync(cancellationToken);
        }
    }

}
