using MediatR;
using RentCarX.Application.DTOs;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.UserContext;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetAll
{
    public class GetAllReservationsQueryHandler : IRequestHandler<GetAllReservationsQuery, List<ReservationDto>>
    {
        private readonly IReservationRepository _reservationRepository; 
        private readonly IUserContextService _userContext;

        public GetAllReservationsQueryHandler(IReservationRepository reservationRepository, IUserContextService userContext) 
        {
            _reservationRepository = reservationRepository;
            _userContext = userContext;
        }

        public async Task<List<ReservationDto>> Handle(GetAllReservationsQuery request, CancellationToken cancellationToken)
        {
            var reservations = await _reservationRepository.GetUserReservations(_userContext.UserId.ToString(), cancellationToken);

            return reservations
               .Select(r => new ReservationDto
               {
                   Id = r.Id,
                   CarId = r.CarId,
                   CarName = $"{r.Car.Brand} {r.Car.Model}",
                   StartDate = r.StartDate,
                   EndDate = r.EndDate,
                   TotalCost = r.TotalCost
               })
               .ToList(); 
        }
    }
}
