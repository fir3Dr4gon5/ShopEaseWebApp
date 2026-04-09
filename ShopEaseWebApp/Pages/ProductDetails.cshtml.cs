using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Models;
using System.Security.Claims;

namespace ShopEaseWebApp.Pages
{
    [Authorize]
    public class ProductDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProductDetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Product Product { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        public IActionResult OnGet(int id)
        {
            Product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (Product == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            Product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (Product == null)
            {
                return NotFound();
            }

            if (Quantity < 1 || Quantity > Product.StockQuantity)
            {
                ModelState.AddModelError(nameof(Quantity),
                    $"Choose a quantity between 1 and {Product.StockQuantity}.");
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var existing = _context.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == id);
            if (existing != null)
            {
                var merged = existing.Quantity + Quantity;
                existing.Quantity = merged > Product.StockQuantity ? Product.StockQuantity : merged;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = id,
                    Quantity = Quantity
                });
            }

            _context.SaveChanges();
            return RedirectToPage("/Cart");
        }
    }
}
