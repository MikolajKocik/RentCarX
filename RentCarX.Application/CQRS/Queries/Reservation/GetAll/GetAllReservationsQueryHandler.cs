using AutoMapper;
using MediatR;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.UserContext;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetAll
{
    public class GetAllReservationsQueryHandler : IRequestHandler<GetAllReservationsQuery, List<ReservationDto>>
    {
        private readonly IReservationRepository _reservationRepository; 
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;   

        public GetAllReservationsQueryHandler(IReservationRepository reservationRepository, IUserContextService userContext,
            IMapper mapper) 
        {
            _reservationRepository = reservationRepository;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<List<ReservationDto>> Handle(GetAllReservationsQuery request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;

            var reservations = await _reservationRepository.GetUserReservations(userId, cancellationToken);

            return _mapper.Map<List<ReservationDto>>(reservations.ToList());
        }
    }
}
