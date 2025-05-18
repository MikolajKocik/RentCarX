using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.CQRS.Commands.Auth.ConfirmEmail;
using RentCarX.Application.CQRS.Commands.Auth.ForgotPassword;
using RentCarX.Application.CQRS.Commands.Auth.Login;
using RentCarX.Application.CQRS.Commands.Auth.Register;
using RentCarX.Application.CQRS.Commands.Auth.ResetPassword;
using RentCarX.Application.DTOs.Auth; 

namespace RentCarX.Presentation.Controllers
{
    [Route("api/auth")] 
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
        {
            var result = await _mediator.Send(new ConfirmEmailCommand(userId, token));
            return result ? Ok("Email confirmed") : BadRequest("Confirmation failed");
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] string email)
        {
            var command = new ForgotPasswordCommand { Email = email };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var success = await _mediator.Send(command);
            return success ? Ok("Password has been reset") : BadRequest("Invalid reset attempt");
        }
    }
}