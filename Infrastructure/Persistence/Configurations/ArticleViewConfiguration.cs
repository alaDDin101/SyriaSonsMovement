using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class ArticleViewConfiguration : IEntityTypeConfiguration<ArticleView>
{
    public void Configure(EntityTypeBuilder<ArticleView> b)
    {
        b.ToTable("ArticleViews");
        b.HasKey(x => new { x.ArticleId, x.IpAddress });

        b.Property(x => x.IpAddress).IsRequired().HasMaxLength(64);
        b.Property(x => x.ViewedAt).IsRequired();

        b.HasIndex(x => x.ViewedAt);

        b.HasOne(x => x.Article)
            .WithMany(x => x.Views)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
