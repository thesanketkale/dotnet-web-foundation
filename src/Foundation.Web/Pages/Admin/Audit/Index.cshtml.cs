using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Audit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foundation.Web.Pages.Admin.Audit;

public class IndexModel : PageModel
{
    private readonly IQueryHandler<GetAuditLogsQuery, Result<PagedList<AuditLogDto>>> _auditHandler;

    public IndexModel(IQueryHandler<GetAuditLogsQuery, Result<PagedList<AuditLogDto>>> auditHandler)
    {
        _auditHandler = auditHandler;
    }

    public PagedList<AuditLogDto>? AuditList { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? EntityName { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? UserId { get; set; }

    public async Task<IActionResult> OnGetAsync(int page = 1)
    {
        var query = new GetAuditLogsQuery(PageNumber: page, PageSize: 25, EntityName: EntityName, UserId: UserId?.ToString());
        var result = await _auditHandler.HandleAsync(query);
        if (result.IsSuccess)
        {
            AuditList = result.Value;
        }

        return Page();
    }
}
