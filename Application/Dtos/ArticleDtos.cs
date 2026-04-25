using SyriaSonsMovement.Application.Common;

namespace SyriaSonsMovement.Application.Dtos;

public sealed class ArticleSummaryDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? Summary { get; init; }
    public string? CoverImageUrl { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public long ViewCount { get; init; }
    public long LikeCount { get; init; }
    public int CommentCount { get; init; }
}

public sealed class ArticleDetailDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? Summary { get; init; }
    public required string BodyHtml { get; init; }
    public string? CoverImageUrl { get; init; }
    public bool IsPublished { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public long ViewCount { get; init; }
    public long LikeCount { get; init; }
    public Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public string? AuthorDisplayName { get; init; }
    public bool LikedByCurrentUser { get; init; }
    public PagedResult<ArticleCommentDto> Comments { get; init; } = new()
    {
        Items = [],
        TotalCount = 0,
        Page = 1,
        PageSize = 1,
    };
    public PagedResult<ArticleLikeUserDto> LikedUsers { get; init; } = new()
    {
        Items = [],
        TotalCount = 0,
        Page = 1,
        PageSize = 1,
    };
}

public sealed class ArticleLikeUserDto
{
    public Guid UserId { get; init; }
    public string? UserDisplayName { get; init; }
    public string? UserEmail { get; init; }
    public DateTimeOffset LikedAt { get; init; }
}

public sealed class ArticleUpsertDto
{
    public Guid CategoryId { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? Summary { get; init; }
    public required string BodyHtml { get; init; }
    public string? CoverImageUrl { get; init; }
    public bool IsPublished { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
}

public sealed class ArticleListItemDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? CoverImageUrl { get; init; }
    public bool IsPublished { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public long ViewCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
