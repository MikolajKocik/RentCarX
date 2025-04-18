using MediatR;
using RentCarX.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Queries.Reservation.GetAll
{
    public record GetMyReservationsQuery : IRequest<List<ReservationDto>>;

}
