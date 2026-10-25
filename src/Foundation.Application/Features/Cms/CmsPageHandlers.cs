using System.Text.RegularExpressions;
using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Domain.Entities;
using Foundation.Domain.Enums;
using Foundation.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Application.Features.Cms;

public static class SlugHelper
{
    public static string NormalizeSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var slug = input.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\-_/]", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
}

public class CmsPageHandlers :
    ICommandHandler<CreatePageCommand, Result<CmsPageDto>>,
    ICommandHandler<UpdatePageCommand, Result<CmsPageDto>>,
    ICommandHandler<DeletePageCommand, Result>,
    IQueryHandler<GetPagesQuery, Result<PagedList<CmsPageDto>>>,
    IQueryHandler<GetPageBySlugQuery, Result<CmsPageDto>>,
    IQueryHandler<GetPageByIdQuery, Result<CmsPageDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMarkdownRenderer _renderer;
    private readonly IDateTimeProvider _dateTime;

    public CmsPageHandlers(
        IAppDbContext db,
        ICurrentUser currentUser,
        IMarkdownRenderer renderer,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _renderer = renderer;
        _dateTime = dateTime;
    }

    public async Task<Result<CmsPageDto>> HandleAsync(CreatePageCommand command, CancellationToken cancellationToken = default)
    {
        var slug = SlugHelper.NormalizeSlug(command.Slug);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = SlugHelper.NormalizeSlug(command.Title);
        }

        bool slugExists = await _db.CmsPages
            .AnyAsync(p => p.Slug == slug, cancellationToken);
        if (slugExists)
        {
            return Result.Failure<CmsPageDto>($"A page with slug '{slug}' already exists.");
        }

        var renderedHtml = _renderer.RenderHtml(command.ContentMarkdown);
        var sanitizedHtml = _renderer.SanitizeHtml(renderedHtml);

        var page = new CmsPage
        {
            Title = command.Title.Trim(),
            Slug = slug,
            Summary = command.Summary?.Trim(),
            ContentMarkdown = command.ContentMarkdown,
            ContentHtml = sanitizedHtml,
            Status = command.Status,
            PublishedAtUtc = command.Status == ContentStatus.Published
                ? command.PublishedAtUtc ?? _dateTime.UtcNow
                : command.PublishedAtUtc,
            AuthorId = _currentUser.UserId ?? Guid.Empty,
            MetaTitle = command.MetaTitle?.Trim(),
            MetaDescription = command.MetaDescription?.Trim(),
            CreatedAtUtc = _dateTime.UtcNow,
            ConcurrencyToken = Guid.NewGuid()
        };

        _db.CmsPages.Add(page);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(page));
    }

    public async Task<Result<CmsPageDto>> HandleAsync(UpdatePageCommand command, CancellationToken cancellationToken = default)
    {
        var page = await _db.CmsPages
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (page == null)
        {
            return Result.Failure<CmsPageDto>("Page not found.");
        }

        if (page.ConcurrencyToken != command.ConcurrencyToken)
        {
            throw new ConcurrencyConflictException(nameof(CmsPage), command.Id);
        }

        var slug = SlugHelper.NormalizeSlug(command.Slug);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = SlugHelper.NormalizeSlug(command.Title);
        }

        bool slugConflict = await _db.CmsPages
            .AnyAsync(p => p.Slug == slug && p.Id != command.Id, cancellationToken);
        if (slugConflict)
        {
            return Result.Failure<CmsPageDto>($"A page with slug '{slug}' already exists.");
        }

        var revision = new CmsRevision
        {
            ContentId = page.Id,
            ContentType = "Page",
            Title = page.Title,
            ContentMarkdown = page.ContentMarkdown,
            AuthorId = _currentUser.UserId ?? page.AuthorId,
            CreatedAtUtc = _dateTime.UtcNow
        };
        _db.CmsRevisions.Add(revision);

        var renderedHtml = _renderer.RenderHtml(command.ContentMarkdown);
        var sanitizedHtml = _renderer.SanitizeHtml(renderedHtml);

        page.Title = command.Title.Trim();
        page.Slug = slug;
        page.Summary = command.Summary?.Trim();
        page.ContentMarkdown = command.ContentMarkdown;
        page.ContentHtml = sanitizedHtml;
        page.Status = command.Status;
        if (command.Status == ContentStatus.Published && page.PublishedAtUtc == null)
        {
            page.PublishedAtUtc = command.PublishedAtUtc ?? _dateTime.UtcNow;
        }
        else
        {
            page.PublishedAtUtc = command.PublishedAtUtc;
        }
        page.MetaTitle = command.MetaTitle?.Trim();
        page.MetaDescription = command.MetaDescription?.Trim();
        page.UpdatedAtUtc = _dateTime.UtcNow;
        page.ConcurrencyToken = Guid.NewGuid();

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(page));
    }

    public async Task<Result> HandleAsync(DeletePageCommand command, CancellationToken cancellationToken = default)
    {
        var page = await _db.CmsPages
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (page == null)
        {
            return Result.Failure("Page not found.");
        }

        page.IsDeleted = true;
        page.DeletedAtUtc = _dateTime.UtcNow;
        page.DeletedBy = _currentUser.UserName ?? _currentUser.UserId?.ToString();

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PagedList<CmsPageDto>>> HandleAsync(GetPagesQuery query, CancellationToken cancellationToken = default)
    {
        var queryable = _db.CmsPages.AsNoTracking();

        if (!query.IncludeUnpublished)
        {
            queryable = queryable.Where(p => p.Status == ContentStatus.Published);
        }

        var total = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(p => p.PublishedAtUtc ?? p.CreatedAtUtc)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedList<CmsPageDto>(items, total, query.PageNumber, query.PageSize));
    }

    public async Task<Result<CmsPageDto>> HandleAsync(GetPageBySlugQuery query, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = SlugHelper.NormalizeSlug(query.Slug);
        var queryable = _db.CmsPages.AsNoTracking();

        if (!query.AllowUnpublished)
        {
            queryable = queryable.Where(p => p.Status == ContentStatus.Published);
        }

        var page = await queryable.FirstOrDefaultAsync(p => p.Slug == normalizedSlug, cancellationToken);
        if (page == null)
        {
            return Result.Failure<CmsPageDto>($"Page with slug '{query.Slug}' not found.");
        }

        return Result.Success(MapToDto(page));
    }

    public async Task<Result<CmsPageDto>> HandleAsync(GetPageByIdQuery query, CancellationToken cancellationToken = default)
    {
        var page = await _db.CmsPages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);
        if (page == null)
        {
            return Result.Failure<CmsPageDto>("Page not found.");
        }

        return Result.Success(MapToDto(page));
    }

    private static CmsPageDto MapToDto(CmsPage p) => new(
        p.Id,
        p.Title,
        p.Slug,
        p.Summary,
        p.ContentMarkdown,
        p.ContentHtml,
        p.Status,
        p.PublishedAtUtc,
        p.AuthorId,
        p.MetaTitle,
        p.MetaDescription,
        p.ConcurrencyToken,
        p.CreatedAtUtc,
        p.UpdatedAtUtc
    );
}
