using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class ProjectsPageSettingsConfiguration : IEntityTypeConfiguration<ProjectsPageSettings>
{
    public void Configure(EntityTypeBuilder<ProjectsPageSettings> b)
    {
        b.ToTable("ProjectsPageSettings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).IsRequired().HasMaxLength(300);
        b.Property(x => x.LeadText).HasMaxLength(2000);
        b.Property(x => x.IntroHtml).HasMaxLength(200000);
    }
}
