using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.DTOs.Notification;
using RentCarX.Application.Interfaces.Services.Notifications;
using RentCarX.Domain.Interfaces.UserContext;

namespace RentCarX.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class NotificationController : ControllerBase
{
    private readonly IUserContextService _userContextService;
    private readonly INotificationRegister _registrationService;

    public NotificationController(IUserContextService userContextService, INotificationRegister registrationService)
    {
        _userContextService = userContextService;
        _registrationService = registrationService;
    }

    [HttpPost("register-device")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest request)
    {
        string email = _userContextService.Email;

        await _registrationService.RegisterDeviceAsync(request.Token, email);

        return Ok();
    }
}
