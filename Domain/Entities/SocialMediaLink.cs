namespace SyriaSonsMovement.Domain.Entities;

public class SocialMediaLink
{
    public Guid Id { get; set; }

    /// <summary>Stable key for UI/theming, e.g. facebook, x, youtube.</summary>
    public string PlatformKey { get; set; } = null!;
    public string? Label { get; set; }
    public string Url { get; set; } = null!;
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
