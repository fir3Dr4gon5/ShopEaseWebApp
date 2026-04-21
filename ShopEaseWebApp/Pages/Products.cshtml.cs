using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public void OnGet()
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var trimmedSearch = SearchTerm.Trim();
                query = query.Where(product =>
                    EF.Functions.Like(product.Name, $"%{trimmedSearch}%") ||
                    EF.Functions.Like(product.Description, $"%{trimmedSearch}%"));
            }

            Products = query.ToList();
        }
    }
}
