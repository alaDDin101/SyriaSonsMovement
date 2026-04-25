using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(256);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(256);
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.BackgroundImageUrl).HasMaxLength(2048);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.HasIndex(x => new { x.IsActive, x.DisplayOrder });
    }
}
