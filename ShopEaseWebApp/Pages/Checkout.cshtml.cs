using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ShopEaseWebApp.Pages
{
    [Authorize]
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CheckoutModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CartItem> CartItems { get; set; }
        public decimal Total { get; set; }

        [BindProperty]
        public string ShippingAddress { get; set; }

        [BindProperty]
        public string City { get; set; }

        [BindProperty]
        public string PostCode { get; set; }

        public void OnGet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            CartItems = _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            Total = 0;
            foreach (var item in CartItems)
            {
                Total += item.Product.Price * item.Quantity;
            }
        }

        public IActionResult OnPost()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            CartItems = _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            if (CartItems.Count == 0)
            {
                return RedirectToPage("/Cart");
            }

            Total = 0;
            foreach (var item in CartItems)
            {
                Total += item.Product.Price * item.Quantity;
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = Total,
                ShippingAddress = ShippingAddress,
                City = City,
                PostCode = PostCode,
                Status = "Pending"
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.Product.Price
                };

                _context.OrderItems.Add(orderItem);

                var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            _context.CartItems.RemoveRange(CartItems);
            _context.SaveChanges();

            return RedirectToPage("/OrderConfirmation", new { orderId = order.Id });
        }
    }
}
