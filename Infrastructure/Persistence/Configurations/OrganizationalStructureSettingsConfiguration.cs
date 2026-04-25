using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class OrganizationalStructureSettingsConfiguration : IEntityTypeConfiguration<OrganizationalStructureSettings>
{
    public void Configure(EntityTypeBuilder<OrganizationalStructureSettings> b)
    {
        b.ToTable("OrganizationalStructureSettings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.LeadText).HasMaxLength(2000);
        b.Property(x => x.IntroHtml).HasMaxLength(200_000);
        b.Property(x => x.UpdatedAt).IsRequired();
    }
}
