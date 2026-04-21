using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Models;

namespace ShopEaseWebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class TicketsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TicketsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Ticket> Tickets { get; private set; } = new();

        public async Task OnGetAsync()
        {
            Tickets = await _context.Tickets
                .Include(t => t.User)
                .OrderByDescending(t => t.SubmittedAtUtc)
                .ToListAsync();
        }
    }
}
