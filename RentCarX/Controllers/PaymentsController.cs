using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using RentCarX.Application.DTOs.Stripe;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Domain.Interfaces.UserContext;

namespace RentCarX.Presentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/payments")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IUserContextService _userContextService;
        private readonly IFeatureManager _featureManager;

        public PaymentsController(
            IPaymentService paymentService,
            IUserContextService userContextService,
            IFeatureManager featureManager
            )
        {
            _paymentService = paymentService;
            _userContextService = userContextService;
            _featureManager = featureManager;
        }

        [HttpPost("checkout/{reservationId:guid}")]
        public async Task<IActionResult> CreateCheckoutSession([FromRoute] Guid reservationId, CancellationToken cancellationToken)
        {
            if (await _featureManager.IsEnabledAsync("Payments"))
            {
                var userId = _userContextService.UserId;
                if (userId == Guid.Empty)
                {
                    return Unauthorized();
                }

                var request = new CreateCheckoutSessionRequest
                {
                    ReservationId = reservationId,
                    UserId = userId
                };

                var checkoutUrl = await _paymentService.CreateCheckoutSessionAsync(request, cancellationToken);

                return Ok(new { Url = checkoutUrl });
            }
            else
            {
                var errorResponse = new { Message = "Stripe payment functionality is not available right now" };
                return StatusCode(StatusCodes.Status503ServiceUnavailable, errorResponse);
            }
        }
    }
}
