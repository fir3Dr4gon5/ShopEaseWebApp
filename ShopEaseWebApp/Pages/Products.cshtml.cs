using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Models;
using System.Security.Claims;

namespace ShopEaseWebApp.Pages
{
    
    public class ProductsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProductsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new();
        public List<Product> RecommendedProducts { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var trimmedSearch = SearchTerm.Trim();
                query = query.Where(product =>
                    EF.Functions.Like(product.Name, $"%{trimmedSearch}%") ||
                    EF.Functions.Like(product.Description, $"%{trimmedSearch}%"));
            }

            Products = await query.ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var purchasedItems = await _context.OrderItems
                .Where(orderItem => orderItem.Order.UserId == userId)
                .Select(orderItem => new { orderItem.ProductId, orderItem.Product.Category })
                .ToListAsync();

            if (!purchasedItems.Any())
            {
                return;
            }

            var purchasedProductIds = purchasedItems
                .Select(item => item.ProductId)
                .Distinct()
                .ToList();

            var topCategories = purchasedItems
                .Where(item => !string.IsNullOrWhiteSpace(item.Category))
                .GroupBy(item => item.Category)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .Take(3)
                .ToList();

            if (!topCategories.Any())
            {
                return;
            }

            RecommendedProducts = await _context.Products
                .Where(product =>
                    topCategories.Contains(product.Category) &&
                    !purchasedProductIds.Contains(product.Id))
                .Take(6)
                .ToListAsync();
        }
    }
}
