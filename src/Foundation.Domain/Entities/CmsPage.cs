using Foundation.Domain.Common;
using Foundation.Domain.Enums;

namespace Foundation.Domain.Entities;

public class CmsPage : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string ContentMarkdown { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime? PublishedAtUtc { get; set; }
    public Guid AuthorId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // Concurrency and tracking
    public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
