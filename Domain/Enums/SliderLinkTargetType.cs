namespace SyriaSonsMovement.Domain.Enums;

/// <summary>
/// Slider click behavior: no link, external URL, or article.
/// </summary>
public enum SliderLinkTargetType : short
{
    /// <summary>Display only; not clickable.</summary>
    None = 0,
    ExternalUrl = 1,
    /// <summary>Navigate to an article; set <c>ArticleId</c> on the slide.</summary>
    InternalArticle = 2,
}
