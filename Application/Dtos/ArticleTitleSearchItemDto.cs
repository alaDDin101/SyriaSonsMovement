namespace SyriaSonsMovement.Application.Dtos;

/// <summary>Lightweight row for dashboard article pickers (e.g. slider internal link).</summary>
public sealed class ArticleTitleSearchItemDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public bool IsPublished { get; init; }
    public string? Summary { get; init; }
    public string? CoverImageUrl { get; init; }
}
