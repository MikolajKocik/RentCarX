using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace RentCarX.Presentation.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly IConfiguration _configuration;

        public StripeWebhookController(ILogger<StripeWebhookController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var endpointSecret = _configuration["Stripe:WebhookSecret"];
            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook signature verification failed.");
                return BadRequest();
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;

                var reservationId = session?.Metadata["reservationId"];
                _logger.LogInformation($"Payment completed for Reservation ID: {reservationId}");

                // TODO: oznaczyć rezerwację jako opłaconą w DB
            }

            return Ok();
        }
    }
}
