using Foundation.Domain.Common;
using Foundation.Domain.Enums;

namespace Foundation.Domain.Entities;

public class AuditEntry : BaseEntity
{
    public string? UserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string ChangesJson { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}
