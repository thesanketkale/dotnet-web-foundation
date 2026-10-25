using Foundation.Domain.Common;

namespace Foundation.Domain.Entities;

public class UserProfile : IAuditableEntity
{
    public Guid Id { get; set; } // Foreign key to ApplicationUser.Id
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public bool IsPrivate { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
