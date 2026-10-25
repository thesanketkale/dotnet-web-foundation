using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Domain.Entities;
using Foundation.Domain.Enums;
using Foundation.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Application.Features.Cms;

public class CmsPostHandlers :
    ICommandHandler<CreatePostCommand, Result<CmsPostDto>>,
    ICommandHandler<UpdatePostCommand, Result<CmsPostDto>>,
    ICommandHandler<DeletePostCommand, Result>,
    IQueryHandler<GetPostsQuery, Result<PagedList<CmsPostDto>>>,
    IQueryHandler<GetPostBySlugQuery, Result<CmsPostDto>>,
    IQueryHandler<GetPostByIdQuery, Result<CmsPostDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMarkdownRenderer _renderer;
    private readonly IDateTimeProvider _dateTime;

    public CmsPostHandlers(
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

    public async Task<Result<CmsPostDto>> HandleAsync(CreatePostCommand command, CancellationToken cancellationToken = default)
    {
        var slug = SlugHelper.NormalizeSlug(command.Slug);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = SlugHelper.NormalizeSlug(command.Title);
        }

        bool slugExists = await _db.CmsPosts
            .AnyAsync(p => p.Slug == slug, cancellationToken);
        if (slugExists)
        {
            return Result.Failure<CmsPostDto>($"A post with slug '{slug}' already exists.");
        }

        var renderedHtml = _renderer.RenderHtml(command.ContentMarkdown);
        var sanitizedHtml = _renderer.SanitizeHtml(renderedHtml);

        var post = new CmsPost
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

        _db.CmsPosts.Add(post);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(post));
    }

    public async Task<Result<CmsPostDto>> HandleAsync(UpdatePostCommand command, CancellationToken cancellationToken = default)
    {
        var post = await _db.CmsPosts
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (post == null)
        {
            return Result.Failure<CmsPostDto>("Post not found.");
        }

        if (post.ConcurrencyToken != command.ConcurrencyToken)
        {
            throw new ConcurrencyConflictException(nameof(CmsPost), command.Id);
        }

        var slug = SlugHelper.NormalizeSlug(command.Slug);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = SlugHelper.NormalizeSlug(command.Title);
        }

        bool slugConflict = await _db.CmsPosts
            .AnyAsync(p => p.Slug == slug && p.Id != command.Id, cancellationToken);
        if (slugConflict)
        {
            return Result.Failure<CmsPostDto>($"A post with slug '{slug}' already exists.");
        }

        var revision = new CmsRevision
        {
            ContentId = post.Id,
            ContentType = "Post",
            Title = post.Title,
            ContentMarkdown = post.ContentMarkdown,
            AuthorId = _currentUser.UserId ?? post.AuthorId,
            CreatedAtUtc = _dateTime.UtcNow
        };
        _db.CmsRevisions.Add(revision);

        var renderedHtml = _renderer.RenderHtml(command.ContentMarkdown);
        var sanitizedHtml = _renderer.SanitizeHtml(renderedHtml);

        post.Title = command.Title.Trim();
        post.Slug = slug;
        post.Summary = command.Summary?.Trim();
        post.ContentMarkdown = command.ContentMarkdown;
        post.ContentHtml = sanitizedHtml;
        post.Status = command.Status;
        if (command.Status == ContentStatus.Published && post.PublishedAtUtc == null)
        {
            post.PublishedAtUtc = command.PublishedAtUtc ?? _dateTime.UtcNow;
        }
        else
        {
            post.PublishedAtUtc = command.PublishedAtUtc;
        }
        post.MetaTitle = command.MetaTitle?.Trim();
        post.MetaDescription = command.MetaDescription?.Trim();
        post.UpdatedAtUtc = _dateTime.UtcNow;
        post.ConcurrencyToken = Guid.NewGuid();

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(post));
    }

    public async Task<Result> HandleAsync(DeletePostCommand command, CancellationToken cancellationToken = default)
    {
        var post = await _db.CmsPosts
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (post == null)
        {
            return Result.Failure("Post not found.");
        }

        post.IsDeleted = true;
        post.DeletedAtUtc = _dateTime.UtcNow;
        post.DeletedBy = _currentUser.UserName ?? _currentUser.UserId?.ToString();

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PagedList<CmsPostDto>>> HandleAsync(GetPostsQuery query, CancellationToken cancellationToken = default)
    {
        var queryable = _db.CmsPosts.AsNoTracking();

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

        return Result.Success(new PagedList<CmsPostDto>(items, total, query.PageNumber, query.PageSize));
    }

    public async Task<Result<CmsPostDto>> HandleAsync(GetPostBySlugQuery query, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = SlugHelper.NormalizeSlug(query.Slug);
        var queryable = _db.CmsPosts.AsNoTracking();

        if (!query.AllowUnpublished)
        {
            queryable = queryable.Where(p => p.Status == ContentStatus.Published);
        }

        var post = await queryable.FirstOrDefaultAsync(p => p.Slug == normalizedSlug, cancellationToken);
        if (post == null)
        {
            return Result.Failure<CmsPostDto>($"Post with slug '{query.Slug}' not found.");
        }

        return Result.Success(MapToDto(post));
    }

    public async Task<Result<CmsPostDto>> HandleAsync(GetPostByIdQuery query, CancellationToken cancellationToken = default)
    {
        var post = await _db.CmsPosts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);
        if (post == null)
        {
            return Result.Failure<CmsPostDto>("Post not found.");
        }

        return Result.Success(MapToDto(post));
    }

    private static CmsPostDto MapToDto(CmsPost p) => new(
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
