using SyriaSonsMovement.Domain.Enums;

namespace SyriaSonsMovement.Domain.Entities;

public class HomeSliderSlide
{
    public Guid Id { get; set; }

    public string? BackgroundImageUrl { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? ContentHtml { get; set; }

    /// <summary>Optional CSS colors (hex/rgba) for the public hero; null = theme defaults.</summary>
    public string? TitleColor { get; set; }
    public string? SubtitleTextColor { get; set; }
    public string? SubtitleBadgeBackgroundColor { get; set; }
    public string? SubtitleBadgeBorderColor { get; set; }
    public string? ContentHtmlColor { get; set; }
    public string? CtaBackgroundColor { get; set; }
    public string? CtaTextColor { get; set; }
    public string? NavArrowBackgroundColor { get; set; }
    public string? NavArrowIconColor { get; set; }
    public string? DotActiveColor { get; set; }
    public string? DotInactiveColor { get; set; }
    public string? OverlayBottomColor { get; set; }
    public string? OverlayMiddleColor { get; set; }
    public string? OverlayTopColor { get; set; }

    public SliderLinkTargetType LinkTargetType { get; set; }
    public Guid? ArticleId { get; set; }
    public Article? Article { get; set; }
    public string? ExternalUrl { get; set; }
    public bool OpenInNewTab { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
