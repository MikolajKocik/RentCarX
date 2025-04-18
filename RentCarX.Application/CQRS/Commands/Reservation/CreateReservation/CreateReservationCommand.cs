using MediatR;

namespace RentCarX.Application.CQRS.Commands.Reservation.CreateReservation
{
    public class CreateReservationCommand : IRequest<Guid>
    {
        public Guid CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
