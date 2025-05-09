using MediatR;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.CQRS.Commands.Auth.Login;
using RentCarX.Application.CQRS.Commands.Auth.Register;
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
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var token = await _mediator.Send(new RegisterUserCommand(dto));
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        { 
            var token = await _mediator.Send(new LoginUserCommand(dto));
            return Ok(new { message = "User logged successfully", token });
        }

    }

}
