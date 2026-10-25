using System.Text.Json;
using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Profiles;
using Foundation.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foundation.Web.Pages.Account.Manage;

public class PersonalDataModel : PageModel
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IQueryHandler<ExportPersonalDataQuery, Result<PersonalDataExportDto>> _exportHandler;
    private readonly ILogger<PersonalDataModel> _logger;

    public PersonalDataModel(
        UserManager<ApplicationUser> userManager,
        IQueryHandler<ExportPersonalDataQuery, Result<PersonalDataExportDto>> exportHandler,
        ILogger<PersonalDataModel> logger)
    {
        _userManager = userManager;
        _exportHandler = exportHandler;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDownloadAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        _logger.LogInformation("User with ID '{UserId}' requested personal data export.", user.Id);

        var exportResult = await _exportHandler.HandleAsync(new ExportPersonalDataQuery(user.Id));
        if (!exportResult.IsSuccess || exportResult.Value == null)
        {
            return BadRequest("Unable to export personal data.");
        }

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(exportResult.Value, JsonOptions);

        Response.Headers.Append("Content-Disposition", "attachment; filename=personal-data.json");
        return File(jsonBytes, "application/json");
    }
}
