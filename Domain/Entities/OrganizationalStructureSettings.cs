namespace SyriaSonsMovement.Domain.Entities;

public class OrganizationalStructureSettings
{
    public Guid Id { get; set; }

    public string Title { get; set; } = "الهيكل التنظيمي";
    public string? LeadText { get; set; }
    public string? IntroHtml { get; set; }

    public bool IsVisible { get; set; } = true;

    public DateTimeOffset UpdatedAt { get; set; }
}
