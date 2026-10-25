using Foundation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foundation.Infrastructure.Persistence.Configurations;

public class CmsRevisionConfiguration : IEntityTypeConfiguration<CmsRevision>
{
    public void Configure(EntityTypeBuilder<CmsRevision> builder)
    {
        builder.ToTable("CmsRevisions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ContentType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(r => new { r.ContentId, r.CreatedAtUtc });
    }
}
