using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Commands.Reservation.DeleteReservation
{
    public record SoftDeleteReservationCommand(Guid Id) : IRequest<Unit>;
}
