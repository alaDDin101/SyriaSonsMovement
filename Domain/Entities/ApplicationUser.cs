using Microsoft.AspNetCore.Identity;

namespace SyriaSonsMovement.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public ICollection<Article> AuthoredArticles { get; set; } = new List<Article>();
    public ICollection<Project> AuthoredProjects { get; set; } = new List<Project>();
    public ICollection<ArticleLike> ArticleLikes { get; set; } = new List<ArticleLike>();
    public ICollection<ArticleComment> ArticleComments { get; set; } = new List<ArticleComment>();
}
