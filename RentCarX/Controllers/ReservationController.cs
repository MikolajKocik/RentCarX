using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.CQRS.Commands.Reservation.CreateReservation;
using RentCarX.Application.CQRS.Queries.Reservation.GetAll;

namespace RentCarX.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReservationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationCommand command, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetMy), new { }, id);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
        {
            var reservations = await _mediator.Send(new GetMyReservationsQuery(), cancellationToken);
            return Ok(reservations);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllReservationsQuery(), cancellationToken);
            return Ok(result);
        }
    }

}
