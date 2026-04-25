using SyriaSonsMovement.Domain.Enums;

namespace SyriaSonsMovement.Application.Dtos;

public sealed class SliderSlideDto
{
    public Guid Id { get; init; }
    public string? BackgroundImageUrl { get; init; }
    public string? Title { get; init; }
    public string? Subtitle { get; init; }
    public string? ContentHtml { get; init; }
    public SliderLinkTargetType LinkTargetType { get; init; }
    public Guid? ArticleId { get; init; }
    /// <summary>Linked article title when <see cref="ArticleId"/> is set (dashboard display).</summary>
    public string? ArticleTitle { get; init; }
    public string? ArticleSlug { get; init; }
    public string? ExternalUrl { get; init; }
    public bool OpenInNewTab { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }

    public string? TitleColor { get; init; }
    public string? SubtitleTextColor { get; init; }
    public string? SubtitleBadgeBackgroundColor { get; init; }
    public string? SubtitleBadgeBorderColor { get; init; }
    public string? ContentHtmlColor { get; init; }
    public string? CtaBackgroundColor { get; init; }
    public string? CtaTextColor { get; init; }
    public string? NavArrowBackgroundColor { get; init; }
    public string? NavArrowIconColor { get; init; }
    public string? DotActiveColor { get; init; }
    public string? DotInactiveColor { get; init; }
    public string? OverlayBottomColor { get; init; }
    public string? OverlayMiddleColor { get; init; }
    public string? OverlayTopColor { get; init; }
}

public sealed class SliderSlideUpsertDto
{
    public string? BackgroundImageUrl { get; init; }
    public string? Title { get; init; }
    public string? Subtitle { get; init; }
    public string? ContentHtml { get; init; }
    public SliderLinkTargetType LinkTargetType { get; init; }
    public Guid? ArticleId { get; init; }
    public string? ExternalUrl { get; init; }
    public bool OpenInNewTab { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;

    public string? TitleColor { get; init; }
    public string? SubtitleTextColor { get; init; }
    public string? SubtitleBadgeBackgroundColor { get; init; }
    public string? SubtitleBadgeBorderColor { get; init; }
    public string? ContentHtmlColor { get; init; }
    public string? CtaBackgroundColor { get; init; }
    public string? CtaTextColor { get; init; }
    public string? NavArrowBackgroundColor { get; init; }
    public string? NavArrowIconColor { get; init; }
    public string? DotActiveColor { get; init; }
    public string? DotInactiveColor { get; init; }
    public string? OverlayBottomColor { get; init; }
    public string? OverlayMiddleColor { get; init; }
    public string? OverlayTopColor { get; init; }
}
