using Foundation.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Web.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<IndexModel> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public List<UserViewModel> Users { get; set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public bool IsLockedOut { get; set; }
        public IList<string> Roles { get; set; } = [];
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
        Users = [];

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isLocked = await _userManager.IsLockedOutAsync(user);

            Users.Add(new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = isLocked,
                Roles = roles
            });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUnlockAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            ErrorMessage = "User not found.";
            return RedirectToPage();
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);

        _logger.LogInformation("Admin unlocked user account {UserId}.", userId);
        StatusMessage = $"User '{user.UserName}' has been unlocked.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            ErrorMessage = "User not found.";
            return RedirectToPage();
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            ErrorMessage = $"Role '{roleName}' does not exist.";
            return RedirectToPage();
        }

        var inRole = await _userManager.IsInRoleAsync(user, roleName);
        if (inRole)
        {
            await _userManager.RemoveFromRoleAsync(user, roleName);
            StatusMessage = $"Removed role '{roleName}' from user '{user.UserName}'.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, roleName);
            StatusMessage = $"Added role '{roleName}' to user '{user.UserName}'.";
        }

        return RedirectToPage();
    }
}
