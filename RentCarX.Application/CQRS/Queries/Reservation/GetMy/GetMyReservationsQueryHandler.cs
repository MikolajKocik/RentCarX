using AutoMapper;
using MediatR;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Domain.Interfaces.Repositories;
using RentCarX.Domain.Interfaces.UserContext;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetMy
{
    public record GetMyReservationsQueryHandler : IRequestHandler<GetMyReservationsQuery, IEnumerable<ReservationDto>>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContextService;

        public GetMyReservationsQueryHandler(
            IReservationRepository reservationRepository,
            IMapper mapper,
            IUserContextService userContextService
            )
        {
            _reservationRepository = reservationRepository;
            _mapper = mapper;
            _userContextService = userContextService;
        }

        public async Task<IEnumerable<ReservationDto>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
        {
            var userId = _userContextService.UserId;

            var userReservations = await _reservationRepository.GetUserReservationsAsync(userId, cancellationToken);

            return userReservations is null 
                ? Enumerable.Empty<ReservationDto>() 
                : _mapper.Map<IEnumerable<ReservationDto>>(userReservations);
        }
    }
}
