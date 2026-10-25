using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Cms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foundation.Web.Pages.Cms;

public class PostViewerModel : PageModel
{
    private readonly IQueryHandler<GetPostBySlugQuery, Result<CmsPostDto>> _postHandler;

    public PostViewerModel(IQueryHandler<GetPostBySlugQuery, Result<CmsPostDto>> postHandler)
    {
        _postHandler = postHandler;
    }

    public CmsPostDto? CmsPost { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return NotFound();
        }

        var result = await _postHandler.HandleAsync(new GetPostBySlugQuery(slug, AllowUnpublished: false));
        if (!result.IsSuccess || result.Value == null)
        {
            return NotFound();
        }

        CmsPost = result.Value;
        ViewData["Title"] = CmsPost.Title;
        return Page();
    }
}
