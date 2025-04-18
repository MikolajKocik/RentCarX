using MediatR;
using RentCarX.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetAll
{
    public class GetMyReservationsQueryHandler : IRequestHandler<GetMyReservationsQuery, List<ReservationDto>>
    {
        private readonly IRentCarX_DbContext _context;
        private readonly IUserContextService _userContext;

        public GetMyReservationsQueryHandler(IRentCarX_DbContext context, IUserContextService userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<List<ReservationDto>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Reservations
                .Where(r => r.UserId == _userContext.UserId)
                .Include(r => r.Car)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    CarId = r.CarId,
                    CarName = $"{r.Car.Brand} {r.Car.Model}",
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    TotalCost = r.TotalCost
                })
                .ToListAsync(cancellationToken);
        }
    }

}
