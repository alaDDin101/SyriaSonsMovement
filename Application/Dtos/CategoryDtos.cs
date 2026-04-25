namespace SyriaSonsMovement.Application.Dtos;

public sealed class CategoryCardDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? BackgroundImageUrl { get; init; }
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CategoryUpsertDto
{
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? BackgroundImageUrl { get; init; }
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
}
