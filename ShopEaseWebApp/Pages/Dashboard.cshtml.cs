using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ShopEaseWebApp.Pages
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DashboardModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public string SelectedRole { get; set; } = string.Empty;

        public List<UserRoleViewModel> Users { get; private set; } = new();
        public List<string> AvailableRoles { get; private set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostUpdateRoleAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                StatusMessage = "User not found.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(SelectedRole))
            {
                StatusMessage = "Please select a role.";
                return RedirectToPage();
            }

            if (!await _roleManager.RoleExistsAsync(SelectedRole))
            {
                StatusMessage = "Selected role does not exist.";
                return RedirectToPage();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                StatusMessage = "Failed to remove existing user roles.";
                return RedirectToPage();
            }

            var addResult = await _userManager.AddToRoleAsync(user, SelectedRole);
            if (!addResult.Succeeded)
            {
                StatusMessage = "Failed to assign the selected role.";
                return RedirectToPage();
            }

            await _userManager.UpdateSecurityStampAsync(user);
            StatusMessage = $"Updated role for {user.Email ?? user.UserName} to {SelectedRole}.";
            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            AvailableRoles = await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(name => name)
                .ToListAsync();

            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            Users = new List<UserRoleViewModel>(users.Count);
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? user.UserName ?? "No email",
                    CurrentRole = roles.FirstOrDefault() ?? "None"
                });
            }
        }

        public class UserRoleViewModel
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string CurrentRole { get; set; } = "None";
        }
    }
}
