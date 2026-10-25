using Foundation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<CmsPage> CmsPages { get; }
    DbSet<CmsPost> CmsPosts { get; }
    DbSet<CmsRevision> CmsRevisions { get; }
    DbSet<MediaFile> MediaFiles { get; }
    DbSet<AuditEntry> AuditEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
