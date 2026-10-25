using Foundation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foundation.Infrastructure.Persistence.Configurations;

public class CmsPageConfiguration : IEntityTypeConfiguration<CmsPage>
{
    public void Configure(EntityTypeBuilder<CmsPage> builder)
    {
        builder.ToTable("CmsPages");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Summary)
            .HasMaxLength(500);

        builder.Property(p => p.MetaTitle)
            .HasMaxLength(100);

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(300);

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(100);

        // Optimistic concurrency token
        builder.Property(p => p.ConcurrencyToken)
            .IsConcurrencyToken()
            .IsRequired();

        // Unique slug index
        builder.HasIndex(p => p.Slug)
            .IsUnique();

        // Status & publish indexing
        builder.HasIndex(p => new { p.Status, p.PublishedAtUtc });

        // Global query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
