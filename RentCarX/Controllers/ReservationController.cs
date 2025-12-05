using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.CQRS.Commands.Reservation.CreateReservation;
using RentCarX.Application.CQRS.Commands.Reservation.DeleteReservation;
using RentCarX.Application.CQRS.Commands.Reservation.InitiatePayment;
using RentCarX.Application.CQRS.Queries.Reservation.GetAll;
using RentCarX.Application.CQRS.Queries.Reservation.GetById;
using RentCarX.Application.CQRS.Queries.Reservation.GetMy;
using RentCarX.Application.DTOs.Reservation;

namespace RentCarX.Presentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/reservations")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public sealed class ReservationController : ControllerBase
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
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { }, id);
        }

        [HttpGet("my-reservations")]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        public async Task<ActionResult<List<ReservationDto>>> GetMyReservations(CancellationToken cancellationToken)
        {
            IEnumerable<ReservationDto> reservations = await _mediator.Send(new GetMyReservationsQuery(), cancellationToken);
            return Ok(reservations);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ReservationDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            ReservationDto reservation = await _mediator.Send(new GetReservationByIdQuery(id), cancellationToken);
            return Ok(reservation);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        public async Task<ActionResult<List<ReservationDto>>> GetAll(CancellationToken cancellationToken)
        {
            List<ReservationDto> result = await _mediator.Send(new GetAllReservationsQuery(), cancellationToken);
            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpDelete("{id:guid}/delete/soft")] // soft-delete approach
        public async Task<IActionResult> SoftDeleteReservation(Guid id, CancellationToken cancellationToken)
        {
            var delete = await _mediator.Send(new SoftDeleteReservationCommand(id), cancellationToken);
            return NoContent();

        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpDelete("{id:guid}/delete")] 
        public async Task<IActionResult> DeleteReservation(Guid id, CancellationToken cancellationToken)    
        {
            var delete = await _mediator.Send(new SoftDeleteReservationCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:guid}/pay")]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        public async Task<IActionResult> InitiatePayment(Guid id, CancellationToken cancellationToken)
        {
            string checkoutUrl = await _mediator.Send(new InitiatePaymentCommand(id), cancellationToken);

            return Ok(new { checkoutUrl }); 
        }
    }
}
