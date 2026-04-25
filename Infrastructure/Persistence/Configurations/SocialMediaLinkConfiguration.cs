using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class SocialMediaLinkConfiguration : IEntityTypeConfiguration<SocialMediaLink>
{
    public void Configure(EntityTypeBuilder<SocialMediaLink> b)
    {
        b.ToTable("SocialMediaLinks");
        b.HasKey(x => x.Id);
        b.Property(x => x.PlatformKey).IsRequired().HasMaxLength(64);
        b.HasIndex(x => x.PlatformKey);
        b.Property(x => x.Label).HasMaxLength(128);
        b.Property(x => x.Url).IsRequired().HasMaxLength(2048);
        b.Property(x => x.IconUrl).HasMaxLength(2048);
        b.HasIndex(x => new { x.IsActive, x.DisplayOrder });
    }
}
