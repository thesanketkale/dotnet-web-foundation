using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Cms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foundation.Web.Pages.Cms;

public class PageViewerModel : PageModel
{
    private readonly IQueryHandler<GetPageBySlugQuery, Result<CmsPageDto>> _pageHandler;

    public PageViewerModel(IQueryHandler<GetPageBySlugQuery, Result<CmsPageDto>> pageHandler)
    {
        _pageHandler = pageHandler;
    }

    public CmsPageDto? CmsPage { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return NotFound();
        }

        var result = await _pageHandler.HandleAsync(new GetPageBySlugQuery(slug, AllowUnpublished: false));
        if (!result.IsSuccess || result.Value == null)
        {
            return NotFound();
        }

        CmsPage = result.Value;
        ViewData["Title"] = CmsPage.Title;
        return Page();
    }
}
