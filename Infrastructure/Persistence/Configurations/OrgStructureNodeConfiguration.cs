using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class OrgStructureNodeConfiguration : IEntityTypeConfiguration<OrgStructureNode>
{
    public void Configure(EntityTypeBuilder<OrgStructureNode> b)
    {
        b.ToTable("OrgStructureNodes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Kind).HasColumnType("smallint").IsRequired();
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.Description).HasMaxLength(50_000);
        b.Property(x => x.HolderName).HasMaxLength(256);
        b.Property(x => x.DisplayOrder).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();

        b.HasIndex(x => new { x.ParentId, x.DisplayOrder });
        b.HasIndex(x => new { x.Kind, x.IsActive, x.DisplayOrder });

        b.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
