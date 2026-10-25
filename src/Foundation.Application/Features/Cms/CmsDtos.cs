using Foundation.Domain.Enums;

namespace Foundation.Application.Features.Cms;

public record CmsPageDto(
    Guid Id,
    string Title,
    string Slug,
    string? Summary,
    string ContentMarkdown,
    string ContentHtml,
    ContentStatus Status,
    DateTime? PublishedAtUtc,
    Guid AuthorId,
    string? MetaTitle,
    string? MetaDescription,
    Guid ConcurrencyToken,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CmsPostDto(
    Guid Id,
    string Title,
    string Slug,
    string? Summary,
    string ContentMarkdown,
    string ContentHtml,
    ContentStatus Status,
    DateTime? PublishedAtUtc,
    Guid AuthorId,
    string? MetaTitle,
    string? MetaDescription,
    Guid ConcurrencyToken,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record MediaFileDto(
    Guid Id,
    string OriginalFileName,
    string StoredFileName,
    string ContentType,
    long FileSizeBytes,
    string PublicUrl,
    Guid UploaderId,
    DateTime CreatedAtUtc
);

// Page Commands & Queries
public record CreatePageCommand(
    string Title,
    string Slug,
    string? Summary,
    string ContentMarkdown,
    ContentStatus Status,
    DateTime? PublishedAtUtc,
    string? MetaTitle,
    string? MetaDescription
);

public record UpdatePageCommand(
    Guid Id,
    string Title,
    string Slug,
    string? Summary,
    string ContentMarkdown,
    ContentStatus Status,
    DateTime? PublishedAtUtc,
    string? MetaTitle,
    string? MetaDescription,
    Guid ConcurrencyToken
);

public record DeletePageCommand(Guid Id);

public record GetPagesQuery(bool IncludeUnpublished = false, int PageNumber = 1, int PageSize = 20);

public record GetPageBySlugQuery(string Slug, bool AllowUnpublished = false);

public record GetPageByIdQuery(Guid Id);

// Post Commands & Queries
public record CreatePostCommand(
    string Title,
    string Slug,
    string? Summary,
    string ContentMarkdown,
    ContentStatus Status,
    DateTime? PublishedAtUtc,
    string? MetaTitle,
    string? MetaDescription
);

public record UpdatePostCommand(
    Guid Id,
    string Title,
    string Slug,
    string? Summary,
    string ContentMarkdown,
    ContentStatus Status,
    DateTime? PublishedAtUtc,
    string? MetaTitle,
    string? MetaDescription,
    Guid ConcurrencyToken
);

public record DeletePostCommand(Guid Id);

public record GetPostsQuery(bool IncludeUnpublished = false, int PageNumber = 1, int PageSize = 20);

public record GetPostBySlugQuery(string Slug, bool AllowUnpublished = false);

public record GetPostByIdQuery(Guid Id);

// Media Commands & Queries
public record UploadMediaCommand(
    Stream FileStream,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes
);

public record DeleteMediaCommand(Guid Id);

public record GetMediaFilesQuery(int PageNumber = 1, int PageSize = 24);
