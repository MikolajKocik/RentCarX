using AutoMapper;
using MediatR;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Domain.Exceptions;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetById
{
    public sealed class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, ReservationDto>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IMapper _mapper;

        public GetReservationByIdQueryHandler(IReservationRepository reservationRepository, IMapper mapper)
        {
            _reservationRepository = reservationRepository;
            _mapper = mapper;
        }

        public async Task<ReservationDto> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(request.Id, cancellationToken);
            if (reservation is null)
            {
                throw new NotFoundException($"Reservation with id:{request.Id} was not found.", nameof(reservation));
            }

            var result = _mapper.Map<ReservationDto>(reservation);
            return result;
        }
    }
}
