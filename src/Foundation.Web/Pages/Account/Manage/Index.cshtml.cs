using System.ComponentModel.DataAnnotations;
using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Profiles;
using Foundation.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foundation.Web.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IQueryHandler<GetUserProfileQuery, Result<UserProfileDto>> _profileQueryHandler;
    private readonly ICommandHandler<UpdateUserProfileCommand, Result<UserProfileDto>> _updateProfileHandler;
    private readonly IFileStorageService _fileStorage;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IQueryHandler<GetUserProfileQuery, Result<UserProfileDto>> profileQueryHandler,
        ICommandHandler<UpdateUserProfileCommand, Result<UserProfileDto>> updateProfileHandler,
        IFileStorageService fileStorage)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _profileQueryHandler = profileQueryHandler;
        _updateProfileHandler = updateProfileHandler;
        _fileStorage = fileStorage;
    }

    public string Username { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters.")]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        public string TimeZone { get; set; } = "UTC";

        public bool IsPrivate { get; set; }

        [Display(Name = "Avatar Image")]
        public IFormFile? AvatarFile { get; set; }
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        Username = user.UserName ?? string.Empty;

        var profileResult = await _profileQueryHandler.HandleAsync(new GetUserProfileQuery(user.Id));
        if (profileResult.IsSuccess && profileResult.Value != null)
        {
            var p = profileResult.Value;
            Input = new InputModel
            {
                DisplayName = p.DisplayName,
                Bio = p.Bio,
                TimeZone = p.TimeZone,
                IsPrivate = p.IsPrivate
            };
            AvatarUrl = p.AvatarUrl;
        }
        else
        {
            Input = new InputModel
            {
                DisplayName = Username
            };
        }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        string? avatarUrl = AvatarUrl;

        // 1. Handle Avatar Upload if provided
        if (Input.AvatarFile != null && Input.AvatarFile.Length > 0)
        {
            try
            {
                using var stream = Input.AvatarFile.OpenReadStream();
                var storedFileName = await _fileStorage.SaveFileAsync(stream, Input.AvatarFile.FileName, Input.AvatarFile.ContentType);
                avatarUrl = $"/api/v1/content/media/{storedFileName}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to upload avatar: {ex.Message}";
                await LoadAsync(user);
                return Page();
            }
        }

        // 2. Update Profile Details
        var updateCommand = new UpdateUserProfileCommand(
            user.Id,
            Input.DisplayName,
            Input.Bio,
            string.IsNullOrWhiteSpace(Input.TimeZone) ? "UTC" : Input.TimeZone,
            Input.IsPrivate,
            avatarUrl);

        var updateResult = await _updateProfileHandler.HandleAsync(updateCommand);
        if (!updateResult.IsSuccess)
        {
            ErrorMessage = updateResult.Error;
            await LoadAsync(user);
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated successfully.";
        return RedirectToPage();
    }
}
