using Foundation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foundation.Infrastructure.Persistence.Configurations;

public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.OriginalFileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(m => m.StoredFileName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.StoragePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.PublicUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.CreatedBy)
            .HasMaxLength(100);

        builder.Property(m => m.UpdatedBy)
            .HasMaxLength(100);

        builder.HasIndex(m => m.StoredFileName).IsUnique();
        builder.HasIndex(m => m.CreatedAtUtc);
    }
}
