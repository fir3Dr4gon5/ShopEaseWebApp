using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ShopEaseWebApp.Models;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Services;
using System.Security.Claims;

namespace ShopEaseWebApp.Pages
{
   

    [Authorize]
    public class OrderConfirmationModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeOrderFinalizationService _orderFinalizationService;

        public OrderConfirmationModel(
            ApplicationDbContext context,
            StripeOrderFinalizationService orderFinalizationService)
        {
            _context = context;
            _orderFinalizationService = orderFinalizationService;
        }

        public Order? Order { get; set; }
        public bool IsPending { get; set; }
        public string? SessionId { get; set; }

        public async Task<IActionResult> OnGetAsync(int? orderId, string? sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            SessionId = sessionId;

            if (orderId.HasValue)
            {
                Order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId.Value && o.UserId == userId);
            }
            else if (!string.IsNullOrWhiteSpace(sessionId))
            {
                Order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.StripeCheckoutSessionId == sessionId && o.UserId == userId);
            }

            if (Order is null && !string.IsNullOrWhiteSpace(sessionId))
            {
                Order = await _orderFinalizationService.FinalizePaidSessionAsync(sessionId, userId);
            }

            if (Order is null && !string.IsNullOrWhiteSpace(sessionId))
            {
                IsPending = true;
                return Page();
            }

            if (Order is null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
