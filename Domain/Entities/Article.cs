namespace SyriaSonsMovement.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid? AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }

    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Summary { get; set; }
    public string BodyHtml { get; set; } = null!;
    public string? CoverImageUrl { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }
    public long ViewCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ArticleLike> Likes { get; set; } = new List<ArticleLike>();
    public ICollection<ArticleComment> Comments { get; set; } = new List<ArticleComment>();
    public ICollection<ArticleView> Views { get; set; } = new List<ArticleView>();
}
