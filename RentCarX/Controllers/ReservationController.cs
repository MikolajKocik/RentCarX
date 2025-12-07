using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.CQRS.Commands.Reservation.CreateReservation;
using RentCarX.Application.CQRS.Commands.Reservation.DeleteReservation;
using RentCarX.Application.CQRS.Commands.Reservation.InitiatePayment;
using RentCarX.Application.CQRS.Queries.Reservation.GetAll;
using RentCarX.Application.CQRS.Queries.Reservation.GetById;
using RentCarX.Application.CQRS.Queries.Reservation.GetMy;
using RentCarX.Application.DTOs.Reservation;
using RentCarX.Application.Interfaces.Services.Reports;
using RentCarX.Application.Services.ReportingService;

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
        private readonly IEnumerable<IReportingService> _reportingService;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(
            IMediator mediator,
            IEnumerable<IReportingService> reportingService,
            ILogger<ReservationController> logger)
        {
            _mediator = mediator;
            _reportingService = reportingService;
            _logger = logger;
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

        [HttpPost("generate-pdf")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GeneratePdfDocument(CancellationToken cancellationToken)
        {
            IReportingService? pdf = _reportingService.First(r => r.DocumentReport.Equals(DocumentReport.Pdf));
            if (pdf is null)
            {
                return BadRequest(new { Message = "PDF generator is not registered." });
            }

            byte[] doc = await pdf.GenerateReport(cancellationToken);

            const string contentType = "application/pdf";
            const string fileName = "reservations.pdf";
            return File(doc, contentType, fileName);

        }

        [HttpPost("generate-xlsx")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateXlsxDocument(CancellationToken cancellationToken)
        {
            IReportingService? xlsx = _reportingService.First(r => r.DocumentReport.Equals(DocumentReport.Xlsx));
            if (xlsx is null)
            {
                _logger.LogWarning("XLSX generator service not found.");
                return BadRequest(new { Message = "XLSX generator is not registered." });
            }

            byte[] doc = await xlsx.GenerateReport(cancellationToken);

            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            const string fileName = "reservations.xlsx";
            return File(doc, contentType, fileName);
        }
    }
}
