using System.ComponentModel.DataAnnotations;
using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Cms;
using Foundation.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Web.Pages.Admin.Pages;

public class EditModel : PageModel
{
    private readonly ICommandHandler<CreatePageCommand, Result<CmsPageDto>> _createPageHandler;
    private readonly ICommandHandler<UpdatePageCommand, Result<CmsPageDto>> _updatePageHandler;
    private readonly IAppDbContext _context;

    public EditModel(
        ICommandHandler<CreatePageCommand, Result<CmsPageDto>> createPageHandler,
        ICommandHandler<UpdatePageCommand, Result<CmsPageDto>> updatePageHandler,
        IAppDbContext context)
    {
        _createPageHandler = createPageHandler;
        _updatePageHandler = updatePageHandler;
        _context = context;
    }

    [FromRoute]
    public Guid? Id { get; set; }

    [BindProperty]
    public PageInputModel Input { get; set; } = new();

    public bool IsNew => !Id.HasValue;

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class PageInputModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens.")]
        public string Slug { get; set; } = string.Empty;

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
            var pageEntity = await _context.CmsPages.FirstOrDefaultAsync(p => p.Id == id.Value);
            if (pageEntity == null)
            {
                return NotFound();
            }

            Input = new PageInputModel
            {
                Title = pageEntity.Title,
                Slug = pageEntity.Slug,
                MarkdownContent = pageEntity.ContentMarkdown,
                PublishImmediately = pageEntity.Status == ContentStatus.Published,
                ConcurrencyToken = pageEntity.ConcurrencyToken
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
            var command = new CreatePageCommand(
                Input.Title,
                Input.Slug,
                null,
                Input.MarkdownContent,
                status,
                publishedAt,
                null,
                null);

            var result = await _createPageHandler.HandleAsync(command);
            if (!result.IsSuccess || result.Value == null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create page.");
                return Page();
            }

            StatusMessage = "Page created successfully.";
            return RedirectToPage("./Index");
        }
        else
        {
            var command = new UpdatePageCommand(
                Id!.Value,
                Input.Title,
                Input.Slug,
                null,
                Input.MarkdownContent,
                status,
                publishedAt,
                null,
                null,
                Input.ConcurrencyToken);

            var result = await _updatePageHandler.HandleAsync(command);
            if (!result.IsSuccess || result.Value == null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update page.");
                return Page();
            }

            StatusMessage = "Page updated successfully.";
            return RedirectToPage("./Index");
        }
    }
}
