namespace SyriaSonsMovement.Domain.Entities;

/// <summary>إعدادات صفحة «المشاريع والمبادرات» (صف واحد).</summary>
public class ProjectsPageSettings
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;
    public string? LeadText { get; set; }
    public string? IntroHtml { get; set; }

    public bool IsVisible { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
