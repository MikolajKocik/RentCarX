using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RentCarX.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarController : ControllerBase
{
    private readonly IMediator _mediator;

    public CarController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilteredCars(
        [FromQuery] string? brand = null,
        [FromQuery] string? model = null,
        [FromQuery] string? fuelType = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? isAvailable = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFilteredCarsQuery(brand, model, fuelType, minPrice, maxPrice, isAvailable);
        var cars = await _mediator.Send(query, cancellationToken);

        return Ok(cars);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCarCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCarById), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCarById(Guid id, CancellationToken cancellationToken)
    {
        var car = await _mediator.Send(new CarDetailsQuery(id), cancellationToken);
        if (car == null) return NotFound();

        return Ok(car);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] EditCarCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCarCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}
