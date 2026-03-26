using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
    }
}
