using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Cms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foundation.Web.Pages.Admin.Posts;

public class IndexModel : PageModel
{
    private readonly IQueryHandler<GetPostsQuery, Result<PagedList<CmsPostDto>>> _getPostsHandler;
    private readonly ICommandHandler<DeletePostCommand, Result> _deletePostHandler;

    public IndexModel(
        IQueryHandler<GetPostsQuery, Result<PagedList<CmsPostDto>>> getPostsHandler,
        ICommandHandler<DeletePostCommand, Result> deletePostHandler)
    {
        _getPostsHandler = getPostsHandler;
        _deletePostHandler = deletePostHandler;
    }

    public PagedList<CmsPostDto>? PostsList { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int page = 1)
    {
        var result = await _getPostsHandler.HandleAsync(new GetPostsQuery(IncludeUnpublished: true, PageNumber: page, PageSize: 20));
        if (result.IsSuccess)
        {
            PostsList = result.Value;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var result = await _deletePostHandler.HandleAsync(new DeletePostCommand(id));
        if (result.IsSuccess)
        {
            StatusMessage = "Blog post deleted successfully.";
        }
        else
        {
            ErrorMessage = result.Error;
        }

        return RedirectToPage();
    }
}
