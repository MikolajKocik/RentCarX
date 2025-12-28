using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarX.Domain.Interfaces.Services.Stripe;

namespace RentCarX.Presentation.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/stripe")]
    public class StripeAdminController : ControllerBase
    {
        private readonly IStripeProductService _stripeProductService;

        public StripeAdminController(IStripeProductService stripeProductService)
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
