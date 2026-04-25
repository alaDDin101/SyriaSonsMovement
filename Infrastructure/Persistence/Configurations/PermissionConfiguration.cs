using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("Permissions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        b.Property(x => x.NormalizedName).IsRequired().HasMaxLength(128);
        b.HasIndex(x => x.NormalizedName).IsUnique();
        b.Property(x => x.Description).HasMaxLength(500);
    }
}
