using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ShopEaseWebApp.Models;
using ShopEaseWebApp.Data;

namespace ShopEaseWebApp.Pages
{
   

    [Authorize]
    public class OrderConfirmationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public OrderConfirmationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Order Order { get; set; }

        public IActionResult OnGet(int orderId)
        {
            Order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (Order == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
