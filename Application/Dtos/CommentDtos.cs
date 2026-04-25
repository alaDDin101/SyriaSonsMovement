namespace SyriaSonsMovement.Application.Dtos;

public sealed class ArticleCommentDto
{
    public Guid Id { get; init; }
    public required string Body { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public Guid UserId { get; init; }
    public string? UserDisplayName { get; init; }
    public Guid? ParentCommentId { get; init; }
    public bool IsApproved { get; init; }
    public IReadOnlyList<ArticleCommentDto> Replies { get; init; } = [];
}

public sealed class ArticleCommentCreateDto
{
    public required string Body { get; init; }
    public Guid? ParentCommentId { get; init; }
}

public sealed class CommentModerationItemDto
{
    public Guid Id { get; init; }
    public Guid ArticleId { get; init; }
    public required string ArticleTitle { get; init; }
    public required string Body { get; init; }
    public bool IsApproved { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public Guid UserId { get; init; }
    public string? UserEmail { get; init; }
}
