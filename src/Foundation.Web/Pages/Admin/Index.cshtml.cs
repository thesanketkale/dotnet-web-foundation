using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Audit;
using Foundation.Infrastructure.Identity;
using Foundation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IQueryHandler<GetAuditLogsQuery, Result<PagedList<AuditLogDto>>> _auditHandler;

    public IndexModel(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IQueryHandler<GetAuditLogsQuery, Result<PagedList<AuditLogDto>>> auditHandler)
    {
        _context = context;
        _userManager = userManager;
        _auditHandler = auditHandler;
    }

    public int TotalUsers { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalPosts { get; private set; }
    public int TotalMedia { get; private set; }
    public IReadOnlyList<AuditLogDto> RecentAudits { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        TotalUsers = await _userManager.Users.CountAsync();
        TotalPages = await _context.CmsPages.CountAsync();
        TotalPosts = await _context.CmsPosts.CountAsync();
        TotalMedia = await _context.MediaFiles.CountAsync();

        var auditResult = await _auditHandler.HandleAsync(new GetAuditLogsQuery(PageNumber: 1, PageSize: 5));
        if (auditResult.IsSuccess && auditResult.Value != null)
        {
            RecentAudits = auditResult.Value.Items;
        }

        return Page();
    }
}
