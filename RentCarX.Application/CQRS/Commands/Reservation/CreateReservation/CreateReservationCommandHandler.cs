using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Commands.Reservation.CreateReservation
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
    {
        private readonly IRentCarX_DbContext _context;
        private readonly IUserContextService _userContext;

        public CreateReservationCommandHandler(IRentCarX_DbContext context, IUserContextService userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            var car = await _context.Cars.FindAsync(request.CarId);
            if (car == null || !car.IsAvailable)
                throw new Exception("Car not available");

            var overlapping = await _context.Reservations.AnyAsync(r =>
                r.CarId == request.CarId &&
                r.EndDate >= request.StartDate &&
                r.StartDate <= request.EndDate, cancellationToken);

            if (overlapping)
                throw new Exception("Car already reserved for this period");

            var days = (request.EndDate - request.StartDate).Days;
            var totalCost = days * car.PricePerDay;

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                CarId = car.Id,
                UserId = _userContext.UserId, // zakładam że UserId masz w serwisie kontekstowym
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalCost = totalCost
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync(cancellationToken);

            return reservation.Id;
        }
    }

}
