using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Cms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foundation.Web.Pages.Admin.Pages;

public class IndexModel : PageModel
{
    private readonly IQueryHandler<GetPagesQuery, Result<PagedList<CmsPageDto>>> _getPagesHandler;
    private readonly ICommandHandler<DeletePageCommand, Result> _deletePageHandler;

    public IndexModel(
        IQueryHandler<GetPagesQuery, Result<PagedList<CmsPageDto>>> getPagesHandler,
        ICommandHandler<DeletePageCommand, Result> deletePageHandler)
    {
        _getPagesHandler = getPagesHandler;
        _deletePageHandler = deletePageHandler;
    }

    public PagedList<CmsPageDto>? PagesList { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int page = 1)
    {
        var result = await _getPagesHandler.HandleAsync(new GetPagesQuery(IncludeUnpublished: true, PageNumber: page, PageSize: 20));
        if (result.IsSuccess)
        {
            PagesList = result.Value;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var result = await _deletePageHandler.HandleAsync(new DeletePageCommand(id));
        if (result.IsSuccess)
        {
            StatusMessage = "Page deleted successfully.";
        }
        else
        {
            ErrorMessage = result.Error;
        }

        return RedirectToPage();
    }
}
