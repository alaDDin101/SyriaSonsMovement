using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class AboutUsSettingsConfiguration : IEntityTypeConfiguration<AboutUsSettings>
{
    public void Configure(EntityTypeBuilder<AboutUsSettings> b)
    {
        b.ToTable("AboutUsSettings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.LeadText).HasMaxLength(2000);
        b.Property(x => x.BodyHtml).HasMaxLength(200_000).IsRequired();
        b.Property(x => x.ImageUrl).HasMaxLength(2048);
        b.Property(x => x.SectionBackgroundColor).HasMaxLength(64);
        b.Property(x => x.CardBackgroundColor).HasMaxLength(64);
        b.Property(x => x.AccentColor).HasMaxLength(64);
        b.Property(x => x.HeadingTextColor).HasMaxLength(64);
        b.Property(x => x.BodyTextColor).HasMaxLength(64);
        b.Property(x => x.MutedTextColor).HasMaxLength(64);
        b.Property(x => x.UpdatedAt).IsRequired();
    }
}
