using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Domain.Enums;

namespace SyriaSonsMovement.Infrastructure.Persistence.Configurations;

public class HomeSliderSlideConfiguration : IEntityTypeConfiguration<HomeSliderSlide>
{
    public void Configure(EntityTypeBuilder<HomeSliderSlide> b)
    {
        b.ToTable("HomeSliderSlides");
        b.HasKey(x => x.Id);

        b.Property(x => x.LinkTargetType)
            .HasConversion<short>();

        b.Property(x => x.BackgroundImageUrl).HasMaxLength(2048);
        b.Property(x => x.Title).HasMaxLength(512);
        b.Property(x => x.Subtitle).HasMaxLength(1024);
        b.Property(x => x.ExternalUrl).HasMaxLength(2048);

        const int colorMax = 64;
        b.Property(x => x.TitleColor).HasMaxLength(colorMax);
        b.Property(x => x.SubtitleTextColor).HasMaxLength(colorMax);
        b.Property(x => x.SubtitleBadgeBackgroundColor).HasMaxLength(colorMax);
        b.Property(x => x.SubtitleBadgeBorderColor).HasMaxLength(colorMax);
        b.Property(x => x.ContentHtmlColor).HasMaxLength(colorMax);
        b.Property(x => x.CtaBackgroundColor).HasMaxLength(colorMax);
        b.Property(x => x.CtaTextColor).HasMaxLength(colorMax);
        b.Property(x => x.NavArrowBackgroundColor).HasMaxLength(colorMax);
        b.Property(x => x.NavArrowIconColor).HasMaxLength(colorMax);
        b.Property(x => x.DotActiveColor).HasMaxLength(colorMax);
        b.Property(x => x.DotInactiveColor).HasMaxLength(colorMax);
        b.Property(x => x.OverlayBottomColor).HasMaxLength(colorMax);
        b.Property(x => x.OverlayMiddleColor).HasMaxLength(colorMax);
        b.Property(x => x.OverlayTopColor).HasMaxLength(colorMax);

        b.HasIndex(x => new { x.IsActive, x.DisplayOrder });

        b.HasOne(x => x.Article)
            .WithMany()
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.SetNull);

        var t = nameof(HomeSliderSlide.LinkTargetType);
        var articleId = nameof(HomeSliderSlide.ArticleId);
        var ext = nameof(HomeSliderSlide.ExternalUrl);
        var none = (short)SliderLinkTargetType.None;
        var url = (short)SliderLinkTargetType.ExternalUrl;
        var art = (short)SliderLinkTargetType.InternalArticle;

        b.HasCheckConstraint(
            "CK_HomeSliderSlides_LinkConsistency",
            $"(\"{t}\" = {none} AND \"{articleId}\" IS NULL AND (\"{ext}\" IS NULL OR \"{ext}\" = '')) OR " +
            $"(\"{t}\" = {url} AND \"{ext}\" IS NOT NULL AND \"{articleId}\" IS NULL) OR " +
            $"(\"{t}\" = {art} AND \"{articleId}\" IS NOT NULL AND (\"{ext}\" IS NULL OR \"{ext}\" = ''))");
    }
}
