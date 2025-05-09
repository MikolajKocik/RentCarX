using MediatR;
using Microsoft.EntityFrameworkCore;
using RentCarX.Application.DTOs;
using RentCarX.Domain.Interfaces.DbContext;
using RentCarX.Domain.Interfaces.UserContext;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetAll
{
    public class GetAllReservationsQueryHandler : IRequestHandler<GetAllReservationsQuery, List<ReservationDto>>
    {
        private readonly IRentCarX_DbContext _context;
        private readonly IUserContextService _userContext;

        public GetAllReservationsQueryHandler(IRentCarX_DbContext context, IUserContextService userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<List<ReservationDto>> Handle(GetAllReservationsQuery request, CancellationToken cancellationToken)
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
