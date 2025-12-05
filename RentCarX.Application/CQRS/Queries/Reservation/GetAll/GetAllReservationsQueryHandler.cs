using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetAll
{
    public class GetAllReservationsQueryHandler : IRequestHandler<GetAllReservationsQuery, List<ReservationDto>>
    {
        private readonly IReservationRepository _reservationRepository; 
        private readonly IMapper _mapper;   

        public GetAllReservationsQueryHandler(
            IReservationRepository reservationRepository,
            IMapper mapper
            ) 
        {
            _reservationRepository = reservationRepository;
            _mapper = mapper;
        }

        public async Task<List<ReservationDto>> Handle(GetAllReservationsQuery request, CancellationToken cancellationToken)
        {
            var reservations = _reservationRepository.GetAll();

            return reservations is null
                ? Array.Empty<ReservationDto>().ToList()
                : _mapper.Map<List<ReservationDto>>(await reservations.ToListAsync(cancellationToken));
        }
    }
}
