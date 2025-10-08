using Microsoft.AspNetCore.Mvc;
using RentCarX.Domain.Interfaces.Services.Stripe;
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
        private readonly IStripeWebhookHandler _stripeWebhookHandler;
        private readonly IPaymentService _paymentService;

        public StripeWebhookController(
            ILogger<StripeWebhookController> logger,
            IConfiguration configuration,
            IStripeWebhookHandler stripeWebhookHandler,
            IPaymentService paymentService
            )
        {
            _logger = logger;
            _configuration = configuration;
            _stripeWebhookHandler = stripeWebhookHandler;
            _paymentService = paymentService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
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

                if (session != null)
                {
                    await _paymentService.HandleCheckoutSessionCompletedAsync(session.Id);
                }
            }

            await _stripeWebhookHandler.HandleEventAsync(stripeEvent, cancellationToken);

            return Ok();
        }
    }
}
