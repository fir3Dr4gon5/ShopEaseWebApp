using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Options;
using ShopEaseWebApp.Services;
using Stripe;
using Stripe.Checkout;

namespace ShopEaseWebApp.Controllers
{
    [ApiController]
    [Route("api/stripe/webhook")]
    [IgnoreAntiforgeryToken]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeOptions _stripeOptions;
        private readonly StripeOrderFinalizationService _orderFinalizationService;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            ApplicationDbContext context,
            IOptions<StripeOptions> stripeOptions,
            StripeOrderFinalizationService orderFinalizationService,
            ILogger<StripeWebhookController> logger)
        {
            _context = context;
            _stripeOptions = stripeOptions.Value;
            _orderFinalizationService = orderFinalizationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"];

            if (string.IsNullOrWhiteSpace(_stripeOptions.WebhookSecret))
            {
                _logger.LogWarning("Stripe webhook secret is missing.");
                return BadRequest();
            }

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    _stripeOptions.WebhookSecret);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature verification failed.");
                return BadRequest();
            }

            if (stripeEvent.Type != EventTypes.CheckoutSessionCompleted)
            {
                return Ok();
            }

            var session = stripeEvent.Data.Object as Session;
            if (session is null)
            {
                return Ok();
            }

            await _orderFinalizationService.FinalizePaidSessionAsync(session.Id);
            return Ok();
        }
    }
}
