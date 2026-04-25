namespace SyriaSonsMovement.Domain.Entities;

public class AboutUsSettings
{
    public Guid Id { get; set; }

    public string Title { get; set; } = "";
    public string? LeadText { get; set; }
    public string BodyHtml { get; set; } = "";
    public string? ImageUrl { get; set; }

    public bool IsVisible { get; set; } = true;

    /// <summary>Optional CSS color (hex/rgb). Null = use site theme (logo-aligned CSS variables).</summary>
    public string? SectionBackgroundColor { get; set; }

    public string? CardBackgroundColor { get; set; }
    public string? AccentColor { get; set; }
    public string? HeadingTextColor { get; set; }
    public string? BodyTextColor { get; set; }
    public string? MutedTextColor { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
