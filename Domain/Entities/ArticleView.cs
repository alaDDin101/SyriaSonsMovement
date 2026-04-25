namespace SyriaSonsMovement.Domain.Entities;

public class ArticleView
{
    public Guid ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public string IpAddress { get; set; } = null!;
    public DateTimeOffset ViewedAt { get; set; }
}
