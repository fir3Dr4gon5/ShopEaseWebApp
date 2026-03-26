using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Models;

namespace ShopEaseWebApp.Pages
{
    
    public class ProductsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProductsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; }

        public void OnGet()
        {
            Products = _context.Products.ToList();
        }
    }
}
