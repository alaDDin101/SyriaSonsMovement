namespace SyriaSonsMovement.Application.Dtos;

public sealed class SocialLinkDto
{
    public Guid Id { get; init; }
    public required string PlatformKey { get; init; }
    public string? Label { get; init; }
    public required string Url { get; init; }
    public string? IconUrl { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
}

public sealed class SocialLinkUpsertDto
{
    public required string PlatformKey { get; init; }
    public string? Label { get; init; }
    public required string Url { get; init; }
    public string? IconUrl { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
}
