using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShopEaseWebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class TicketsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
