using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.CQRS.Commands.Auth.ConfirmEmail;
using RentCarX.Application.CQRS.Commands.Auth.DeleteAccount;
using RentCarX.Application.CQRS.Commands.Auth.ForgotPassword;
using RentCarX.Application.CQRS.Commands.Auth.Login;
using RentCarX.Application.CQRS.Commands.Auth.Logout;
using RentCarX.Application.CQRS.Commands.Auth.Register;
using RentCarX.Application.CQRS.Commands.Auth.ResetPassword;
using RentCarX.Application.CQRS.Queries.Admin.GetDeletedUsers;
using RentCarX.Application.DTOs.Auth;

namespace RentCarX.Presentation.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[AllowAnonymous]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)] 
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterUserResponseDto>> Register([FromBody] RegisterUserDto dto)
    {
        var command = new RegisterUserCommand(dto);

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var command = new LoginUserCommand(dto);

        var token = await _mediator.Send(command);

        return Ok(new { message = "User logged successfully", token = token });
    }

    [HttpPost("logout")]
    [Authorize] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await _mediator.Send(new LogoutCommand());
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
    {
        var result = await _mediator.Send(new ConfirmEmailCommand(userId, token));
        return Ok($"Result: {result}");
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)] 
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Invalid request");

        var command = new ForgotPasswordCommand(request.Email);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var success = await _mediator.Send(command);
        return success ? Ok("Password has been reset") : BadRequest("Invalid reset attempt");
    }

    [HttpDelete("delete-account")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
    {
        await _mediator.Send(new DeleteAccountUserCommand(dto));
        return Ok(new { message = "User account deleted successfully" });
    }

    [HttpGet("deleted-users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<User>>> GetSoftDeletedUsers(CancellationToken ct)
    {
        IEnumerable<User> users = await _mediator.Send(new GetDeletedUsersQuery());
        return Ok(users.Select(u => 
            new UserDto { Id = u.Id, Email = u.Email, DeletedAt = u.DeletedAt }));
    }

    public sealed record ConfirmEmailRequest(Guid UserId, string Token);
    public sealed record ForgotPasswordRequest(string Email);
}