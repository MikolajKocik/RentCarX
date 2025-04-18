using MediatR;
using RentCarX.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Queries.Car.GetDetails
{
    public class CarDetailsQueryHandler : IRequestHandler<CarDetailsQuery, CarDto?>
    {
        private readonly IRentCarX_DbContext _context;

        public CarDetailsQueryHandler(IRentCarX_DbContext context)
        {
            _context = context;
        }

        public async Task<CarDto?> Handle(CarDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Cars
                .Where(c => c.Id == request.Id)
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    Model = c.Model,
                    Year = c.Year,
                    FuelType = c.FuelType,
                    PricePerDay = c.PricePerDay,
                    IsAvailable = c.IsAvailable
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }

}
