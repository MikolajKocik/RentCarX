using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RentCarX.Domain.Interfaces.Services.Stripe;
using RentCarX.Infrastructure.Settings;
using Stripe;

namespace RentCarX.Presentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly IStripeWebhookHandler _stripeWebhookHandler;
        private readonly string _endpointSecret;

        public StripeWebhookController(
            ILogger<StripeWebhookController> logger,
            IOptions<StripeSettings> stripeSettings,
            IStripeWebhookHandler stripeWebhookHandler)
        {
            _logger = logger;
            _stripeWebhookHandler = stripeWebhookHandler;
            _endpointSecret = stripeSettings.Value.WebhookSecret 
                ?? throw new ArgumentNullException(nameof(stripeSettings.Value.WebhookSecret), "Stripe Webhook Secret is not configured.");
        }

        [HttpPost("webhook")]
        //[Consumes("application/json")]
        public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
        {
            // allows body to be read more than once
            HttpContext.Request.EnableBuffering();

            string json;
            using (var reader = new StreamReader(HttpContext.Request.Body, leaveOpen: true))
            {
                json = await reader.ReadToEndAsync();
            }

            // reset stream so Stripe can read it again if needed
            HttpContext.Request.Body.Position = 0;

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _endpointSecret,
                    throwOnApiVersionMismatch: false
                );

                _logger.LogInformation("Stripe webhook event received: {EventType}", stripeEvent.Type);
            }
            catch (StripeException ex)
            {
                _logger.LogError("Stripe verification failed. Secret Length: {Len}. Error: {Msg}",
                    _endpointSecret?.Length, ex.Message);
                return BadRequest("Invalid signature");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing webhook.");
                return StatusCode(500);
            }

            try
            {
                await _stripeWebhookHandler.HandleEventAsync(stripeEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during webhook handling logic.");
                return Ok();
            }

            return Ok();
        }
    }
}
