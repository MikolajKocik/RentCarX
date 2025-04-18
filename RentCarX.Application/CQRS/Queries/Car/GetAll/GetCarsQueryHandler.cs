using MediatR;
using Microsoft.EntityFrameworkCore;
using RentCarX.Application.DTOs;
using RentCarX.Domain.Interfaces.DbContext;

namespace RentCarX.Application.CQRS.Queries.Car.GetAll
{
    public class GetCarsQueryHandler : IRequestHandler<GetCarsQuery, List<CarDto>>
    {
        private readonly IRentCarX_DbContext _context;

        public GetCarsQueryHandler(IRentCarX_DbContext context)
        {
            _context = context;
        }

        public async Task<List<CarDto>> Handle(GetCarsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Cars.AsQueryable();

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
                    IsAvailable = c.IsAvailable
                })
                .ToListAsync(cancellationToken);

            return result;
        }
    }

}
