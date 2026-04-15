using Microsoft.EntityFrameworkCore;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Models;
using Stripe.Checkout;

namespace ShopEaseWebApp.Services
{
    public class StripeOrderFinalizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StripeOrderFinalizationService> _logger;

        public StripeOrderFinalizationService(
            ApplicationDbContext context,
            ILogger<StripeOrderFinalizationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order?> FinalizePaidSessionAsync(string sessionId, string? expectedUserId = null)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return null;
            }

            var existingOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.StripeCheckoutSessionId == sessionId);
            if (existingOrder is not null)
            {
                return expectedUserId is null || existingOrder.UserId == expectedUserId
                    ? existingOrder
                    : null;
            }

            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(sessionId);
            if (session is null || !string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!session.Metadata.TryGetValue("userId", out var userId) || string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Stripe session {SessionId} missing userId metadata.", sessionId);
                return null;
            }

            if (!string.IsNullOrWhiteSpace(expectedUserId) &&
                !string.Equals(expectedUserId, userId, StringComparison.Ordinal))
            {
                _logger.LogWarning("Stripe session {SessionId} user mismatch.", sessionId);
                return null;
            }

            var shippingAddress = session.Metadata.TryGetValue("shippingAddress", out var address) ? address : string.Empty;
            var city = session.Metadata.TryGetValue("city", out var cityValue) ? cityValue : string.Empty;
            var postCode = session.Metadata.TryGetValue("postCode", out var postCodeValue) ? postCodeValue : string.Empty;
            var cartMetadata = session.Metadata.TryGetValue("cart", out var cartValue) ? cartValue : string.Empty;

            var cartLines = ParseCartMetadata(cartMetadata);
            if (cartLines.Count == 0)
            {
                _logger.LogWarning("Stripe session {SessionId} cart metadata is empty.", sessionId);
                return null;
            }

            var productIds = cartLines.Select(x => x.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var totalAmount = (session.AmountTotal ?? 0) / 100m;
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                ShippingAddress = shippingAddress,
                City = city,
                PostCode = postCode,
                Status = "Confirmed",
                PaymentStatus = "Paid",
                StripeCheckoutSessionId = session.Id,
                StripePaymentIntentId = session.PaymentIntentId
            };

            _context.Orders.Add(order);

            var stockIssueFound = false;
            foreach (var line in cartLines)
            {
                if (!products.TryGetValue(line.ProductId, out var product))
                {
                    stockIssueFound = true;
                    continue;
                }

                if (product.StockQuantity < line.Quantity)
                {
                    stockIssueFound = true;
                    continue;
                }

                _context.OrderItems.Add(new OrderItem
                {
                    Order = order,
                    ProductId = product.Id,
                    Quantity = line.Quantity,
                    PriceAtPurchase = product.Price
                });

                product.StockQuantity -= line.Quantity;
            }

            if (stockIssueFound)
            {
                order.Status = "Payment Received - Stock Review";
            }

            var userCartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();
            _context.CartItems.RemoveRange(userCartItems);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var racedOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.StripeCheckoutSessionId == sessionId);
                return racedOrder;
            }

            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);
        }

        private static List<(int ProductId, int Quantity)> ParseCartMetadata(string cartMetadata)
        {
            var result = new List<(int ProductId, int Quantity)>();
            if (string.IsNullOrWhiteSpace(cartMetadata))
            {
                return result;
            }

            var entries = cartMetadata.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var parts = entry.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    continue;
                }

                if (int.TryParse(parts[0], out var productId) &&
                    int.TryParse(parts[1], out var quantity) &&
                    productId > 0 &&
                    quantity > 0)
                {
                    result.Add((productId, quantity));
                }
            }

            return result;
        }
    }
}
