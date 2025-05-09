using MediatR;
using RentCarX.Domain.Interfaces.DbContext;

namespace RentCarX.Application.CQRS.Commands.Car.EditCar
{
    public class EditCarCommandHandler : IRequestHandler<EditCarCommand> 
    {
        private readonly IRentCarX_DbContext _context;

        public EditCarCommandHandler(IRentCarX_DbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EditCarCommand request, CancellationToken cancellationToken) 
        {
            var carEntity = await _context.Cars.FindAsync(new object[] { request.Id }, cancellationToken);

            if (carEntity == null)
            {
                throw new KeyNotFoundException($"Car with ID {request.Id} not found.");
            }

            if (request.Brand != null) carEntity.Brand = request.Brand;
            if (request.Model != null) carEntity.Model = request.Model;
            if (request.Year.HasValue) carEntity.Year = request.Year.Value;
            if (request.FuelType != null) carEntity.FuelType = request.FuelType;
            if (request.PricePerDay.HasValue) carEntity.PricePerDay = request.PricePerDay.Value;
            if (request.IsAvailable.HasValue) carEntity.IsAvailable = request.IsAvailable.Value;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value; 
        }
    }
}