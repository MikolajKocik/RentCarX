using Microsoft.AspNetCore.Mvc;
using RentCarX.Application.DTOs.Stripe;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Domain.Models;

namespace RentCarX.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-checkout-session")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request, CancellationToken cancellationToken)
        {
            var sessionUrl = await _paymentService.CreateCheckoutSessionAsync(request, cancellationToken);
            return Ok(new { url = sessionUrl });
        }
    }
}
