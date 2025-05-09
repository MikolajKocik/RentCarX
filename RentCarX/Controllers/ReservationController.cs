using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.CQRS.Commands.Reservation.CreateReservation;
using RentCarX.Application.CQRS.Commands.Reservation.InitiatePayment;
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status409Conflict)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] 
        public async Task<IActionResult> Create([FromBody] CreateReservationCommand command, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetMy), new { }, id);
        }

        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
        {
            var reservations = await _mediator.Send(new GetAllReservationsQuery(), cancellationToken);
            return Ok(reservations);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllReservationsQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:guid}/pay")]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InitiatePayment(Guid id, CancellationToken cancellationToken)
        {
            var checkoutUrl = await _mediator.Send(new InitiatePaymentCommand(id), cancellationToken);

            return Ok(new { checkoutUrl }); 
        }
    }
}
