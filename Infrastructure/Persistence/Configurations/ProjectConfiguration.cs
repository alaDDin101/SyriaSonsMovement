using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.ToTable("Projects");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).IsRequired().HasMaxLength(512);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(512);
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Summary).HasMaxLength(4000);
        b.Property(x => x.BodyHtml).IsRequired();
        b.Property(x => x.CoverImageUrl).HasMaxLength(2048);
        b.Property(x => x.Section).HasConversion<byte>();
        b.Property(x => x.ViewCount).HasDefaultValue(0L);
        b.HasIndex(x => new { x.Section, x.IsPublished, x.DisplayOrder });
        b.HasIndex(x => new { x.IsPublished, x.PublishedAt });

        b.HasOne(x => x.Author)
            .WithMany(x => x.AuthoredProjects)
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
