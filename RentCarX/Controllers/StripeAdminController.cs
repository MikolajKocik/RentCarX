using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Infrastructure.Services.Stripe;

namespace RentCarX.Presentation.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/stripe")]
    public class StripeAdminController : ControllerBase
    {
        private readonly StripeProductService _stripeProductService;

        public StripeAdminController(StripeProductService stripeProductService)
        {
            _stripeProductService = stripeProductService;
        }

        [HttpPost("sync-products")]
        public async Task<IActionResult> SyncProducts(CancellationToken cancellationToken)
        {
            await _stripeProductService.SyncProductsFromCarsAsync(cancellationToken);
            return Ok(new { message = "Stripe products synchronized successfully." });
        }
    }

}
