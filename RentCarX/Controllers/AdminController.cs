using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.CQRS.Queries.Admin.GetCarStatuses;
using RentCarX.Application.CQRS.Queries.Admin.GetDeadlineReservations;
using RentCarX.Application.CQRS.Queries.Admin.GetPendingReservations;
using RentCarX.Application.CQRS.Queries.Admin.GetSystemLockedCarIds;
using RentCarX.Application.CQRS.Queries.Admin.GetUnavailableCars;
using RentCarX.Application.DTOs.Car;
using RentCarX.Domain.Models;

namespace RentCarX.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/monitor/")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ProducesResponseType(StatusCodes.Status200OK)]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("unavailable-cars")]
    public async Task<ActionResult<List<CarStatusDto>>> GetUnavailableCars(CancellationToken cancellationToken)
    {
        List<CarStatusDto> cars = await _mediator.Send(new GetUnavailableCarsQuery(), cancellationToken);
        return Ok(cars);
    }

    [HttpGet("pending-reservations")]
    public async Task<ActionResult<List<Reservation>>> GetPendingReservations(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPendingReservationsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("cars-statuses")]
    public async Task<IActionResult> GetCarsStatuses(CancellationToken cancellationToken)
    {
        var statuses = await _mediator.Send(new GetCarsStatusesQuery(), cancellationToken);
        return Ok(statuses);
    }

    [HttpGet("reservations-deadline-today")]
    public async Task<IActionResult> GetReservationsEndingToday(CancellationToken cancellationToken)
    {
        var deadline = await _mediator.Send(new GetDeadlineReservationsQuery(), cancellationToken);
        return Ok(deadline);
    }

    [HttpGet("system-locked-ids")]
    public async Task<IActionResult> GetSystemLockedCarIds(CancellationToken cancellationToken)
    {
        var lockedIds = await _mediator.Send(new GetSystemLockedCarIdsQuery(), cancellationToken);

        return Ok(new
        {
            Description = "IDs of cars currently locked by active or pending-payment reservations",
            Count = lockedIds.Count,
            LockedCarIds = lockedIds
        }); 
    }
}