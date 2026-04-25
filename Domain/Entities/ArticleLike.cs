namespace SyriaSonsMovement.Domain.Entities;

/// <summary>
/// One authenticated user may like an article once (enforced by composite key).
/// </summary>
public class ArticleLike
{
    public Guid ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
