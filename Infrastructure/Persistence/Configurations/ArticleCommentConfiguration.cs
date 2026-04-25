using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class ArticleCommentConfiguration : IEntityTypeConfiguration<ArticleComment>
{
    public void Configure(EntityTypeBuilder<ArticleComment> b)
    {
        b.ToTable("ArticleComments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Body).IsRequired().HasMaxLength(8000);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.IsApproved).HasDefaultValue(false);

        b.HasIndex(x => new { x.ArticleId, x.CreatedAt });

        b.HasOne(x => x.Article)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.User)
            .WithMany(x => x.ArticleComments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        //b.HasOne(x => x.Parent)
        //    .WithMany(x => x.Replies)
        //    .HasForeignKey(x => x.ParentCommentId)
        //    .OnDelete(DeleteBehavior.Restrict);
    }
}
