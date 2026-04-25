using SyriaSonsMovement.Domain.Enums;

namespace SyriaSonsMovement.Domain.Entities;

/// <summary>مشروع أو مبادرة — محتوى غني مشابه للمقال لكن مخزّن ومُعرَض بشكل مستقل.</summary>
public class Project
{
    public Guid Id { get; set; }

    public Guid? AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }

    public ProjectSection Section { get; set; }

    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Summary { get; set; }
    public string BodyHtml { get; set; } = null!;
    public string? CoverImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPublished { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public long ViewCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
