using System.ComponentModel.DataAnnotations;
using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Cms;
using Foundation.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Web.Pages.Admin.Posts;

public class EditModel : PageModel
{
    private readonly ICommandHandler<CreatePostCommand, Result<CmsPostDto>> _createPostHandler;
    private readonly ICommandHandler<UpdatePostCommand, Result<CmsPostDto>> _updatePostHandler;
    private readonly IAppDbContext _context;

    public EditModel(
        ICommandHandler<CreatePostCommand, Result<CmsPostDto>> createPostHandler,
        ICommandHandler<UpdatePostCommand, Result<CmsPostDto>> updatePostHandler,
        IAppDbContext context)
    {
        _createPostHandler = createPostHandler;
        _updatePostHandler = updatePostHandler;
        _context = context;
    }

    [FromRoute]
    public Guid? Id { get; set; }

    [BindProperty]
    public PostInputModel Input { get; set; } = new();

    public bool IsNew => !Id.HasValue;

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class PostInputModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens.")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Summary { get; set; }

        [Required]
        public string MarkdownContent { get; set; } = string.Empty;

        public bool PublishImmediately { get; set; }

        public Guid ConcurrencyToken { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id.HasValue)
        {
            Id = id.Value;
            var postEntity = await _context.CmsPosts.FirstOrDefaultAsync(p => p.Id == id.Value);
            if (postEntity == null)
            {
                return NotFound();
            }

            Input = new PostInputModel
            {
                Title = postEntity.Title,
                Slug = postEntity.Slug,
                Summary = postEntity.Summary,
                MarkdownContent = postEntity.ContentMarkdown,
                PublishImmediately = postEntity.Status == ContentStatus.Published,
                ConcurrencyToken = postEntity.ConcurrencyToken
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var status = Input.PublishImmediately ? ContentStatus.Published : ContentStatus.Draft;
        var publishedAt = Input.PublishImmediately ? DateTime.UtcNow : (DateTime?)null;

        if (IsNew)
        {
            var command = new CreatePostCommand(
                Input.Title,
                Input.Slug,
                Input.Summary,
                Input.MarkdownContent,
                status,
                publishedAt,
                null,
                null);

            var result = await _createPostHandler.HandleAsync(command);
            if (!result.IsSuccess || result.Value == null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create blog post.");
                return Page();
            }

            StatusMessage = "Blog post created successfully.";
            return RedirectToPage("./Index");
        }
        else
        {
            var command = new UpdatePostCommand(
                Id!.Value,
                Input.Title,
                Input.Slug,
                Input.Summary,
                Input.MarkdownContent,
                status,
                publishedAt,
                null,
                null,
                Input.ConcurrencyToken);

            var result = await _updatePostHandler.HandleAsync(command);
            if (!result.IsSuccess || result.Value == null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update blog post.");
                return Page();
            }

            StatusMessage = "Blog post updated successfully.";
            return RedirectToPage("./Index");
        }
    }
}
