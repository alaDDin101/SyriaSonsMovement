namespace SyriaSonsMovement.Domain.Entities;

public class ArticleComment
{
    public Guid Id { get; set; }

    public Guid ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid? ParentCommentId { get; set; }
    public ArticleComment? Parent { get; set; }
    public ICollection<ArticleComment> Replies { get; set; } = new List<ArticleComment>();

    public string Body { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public bool IsApproved { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
