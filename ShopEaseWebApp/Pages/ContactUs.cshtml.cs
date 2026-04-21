using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopEaseWebApp.Data;
using ShopEaseWebApp.Models;
using System.Security.Claims;

namespace ShopEaseWebApp.Pages
{
    [Authorize]
    public class ContactUsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ContactUsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TicketInputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var ticket = new Ticket
            {
                Title = Input.Title.Trim(),
                Message = Input.Message.Trim(),
                SubmittedAtUtc = DateTime.UtcNow,
                UserId = userId
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            StatusMessage = "Your ticket has been submitted. Our team will review it shortly.";
            return RedirectToPage();
        }

        public class TicketInputModel
        {
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.StringLength(120)]
            public string Title { get; set; } = string.Empty;

            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.StringLength(4000)]
            public string Message { get; set; } = string.Empty;
        }
    }
}
