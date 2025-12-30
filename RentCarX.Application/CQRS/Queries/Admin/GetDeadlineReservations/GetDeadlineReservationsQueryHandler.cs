using MediatR;
using Microsoft.EntityFrameworkCore;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Domain.Interfaces.Repositories;

namespace RentCarX.Application.CQRS.Queries.Admin.GetDeadlineReservations;

public sealed class GetDeadlineReservationsQueryHandler : IRequestHandler<GetDeadlineReservationsQuery, List<ReservationDeadlineDto>>
{
    private readonly IReservationRepository _reservationRepository;

    public GetDeadlineReservationsQueryHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }
    
    public async Task<List<ReservationDeadlineDto>> Handle(GetDeadlineReservationsQuery request, CancellationToken cancellationToken)
    {
        var reservations = _reservationRepository
            .GetAll()
            .Where(r => r.EndDate.Date == DateTime.UtcNow.Date);

        return await reservations.Select(r => new ReservationDeadlineDto
        {
            Id = r.Id,
            CarId = r.CarId,
            CarName = $"{r.Car.Brand} {r.Car.Model}",
            Deadline = r.EndDate

        }).ToListAsync(cancellationToken);
    }
}
