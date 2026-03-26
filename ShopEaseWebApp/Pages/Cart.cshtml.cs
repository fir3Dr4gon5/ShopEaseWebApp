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
    public class CartModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CartModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CartItem> CartItems { get; set; }
        public decimal Total { get; set; }

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

        public IActionResult OnPostRemove(int cartItemId)
        {
            var cartItem = _context.CartItems.FirstOrDefault(c => c.Id == cartItemId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }
    }
}
