using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ShopEaseWebApp.Options;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Models;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Security.Claims;

namespace ShopEaseWebApp.Pages
{
    [Authorize]
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeOptions _stripeOptions;

        public CheckoutModel(ApplicationDbContext context, IOptions<StripeOptions> stripeOptions)
        {
            _context = context;
            _stripeOptions = stripeOptions.Value;
        }

        public List<CartItem> CartItems { get; set; } = new();
        public decimal Total { get; set; }
        public bool PaymentCancelled { get; private set; }

        [BindProperty]
        public string ShippingAddress { get; set; } = string.Empty;

        [BindProperty]
        public string City { get; set; } = string.Empty;

        [BindProperty]
        public string PostCode { get; set; } = string.Empty;

        public IActionResult OnGet(bool canceled = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            PaymentCancelled = canceled;
            LoadCart(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            LoadCart(userId);

            if (CartItems.Count == 0)
            {
                return RedirectToPage("/Cart");
            }

            if (string.IsNullOrWhiteSpace(ShippingAddress))
            {
                ModelState.AddModelError(nameof(ShippingAddress), "Shipping address is required.");
            }

            if (string.IsNullOrWhiteSpace(City))
            {
                ModelState.AddModelError(nameof(City), "City is required.");
            }

            if (string.IsNullOrWhiteSpace(PostCode))
            {
                ModelState.AddModelError(nameof(PostCode), "Post code is required.");
            }

            if (string.IsNullOrWhiteSpace(_stripeOptions.SecretKey))
            {
                ModelState.AddModelError(string.Empty, "Stripe is not configured. Add Stripe secrets first.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var cartMetadata = string.Join(",", CartItems.Select(ci => $"{ci.ProductId}:{ci.Quantity}"));
            var lineItems = CartItems.Select(item => new SessionLineItemOptions
            {
                Quantity = item.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "gbp",
                    UnitAmount = (long)Math.Round(item.Product.Price * 100m),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Name
                    }
                }
            }).ToList();

            var successUrl = $"{Request.Scheme}://{Request.Host}/OrderConfirmation?sessionId={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{Request.Scheme}://{Request.Host}/Checkout?canceled=true";

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                LineItems = lineItems,
                Metadata = new Dictionary<string, string>
                {
                    ["userId"] = userId,
                    ["shippingAddress"] = ShippingAddress,
                    ["city"] = City,
                    ["postCode"] = PostCode,
                    ["cart"] = cartMetadata
                }
            };

            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(options);

            return Redirect(session.Url ?? "/Checkout");
        }

        private void LoadCart(string userId)
        {
            CartItems = _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            Total = CartItems.Sum(item => item.Product.Price * item.Quantity);
        }
    }
}
