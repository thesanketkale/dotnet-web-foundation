using Foundation.Domain.Common;

namespace Foundation.Domain.Entities;

public class CmsRevision : BaseEntity
{
    public Guid ContentId { get; set; }
    public string ContentType { get; set; } = string.Empty; // "Page" or "Post"
    public string Title { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
