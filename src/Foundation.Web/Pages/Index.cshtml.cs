using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Cms;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foundation.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IQueryHandler<GetPostsQuery, Result<PagedList<CmsPostDto>>> _postsHandler;

    public IReadOnlyList<CmsPostDto> LatestPosts { get; private set; } = [];

    public IndexModel(IQueryHandler<GetPostsQuery, Result<PagedList<CmsPostDto>>> postsHandler)
    {
        _postsHandler = postsHandler;
    }

    public async Task OnGetAsync()
    {
        var result = await _postsHandler.HandleAsync(new GetPostsQuery(IncludeUnpublished: false, PageNumber: 1, PageSize: 3));
        if (result.IsSuccess)
        {
            LatestPosts = result.Value.Items;
        }
    }
}
