using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.DTOs.Stripe;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Domain.Interfaces.UserContext;

namespace RentCarX.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IUserContextService _userContextService;

        public PaymentsController(IPaymentService paymentService, IUserContextService userContextService)
        {
            _paymentService = paymentService;
            _userContextService = userContextService;
        }

        [HttpPost("checkout/{reservationId}")]
        public async Task<IActionResult> CreateCheckoutSession(Guid reservationId, CancellationToken cancellationToken)
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
    }
}
