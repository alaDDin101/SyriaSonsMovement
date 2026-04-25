namespace SyriaSonsMovement.Application.Dtos;

public sealed class AboutUsPublicDto
{
    public required string Title { get; init; }
    public string? LeadText { get; init; }
    public required string BodyHtml { get; init; }
    public string? ImageUrl { get; init; }
    public string? SectionBackgroundColor { get; init; }
    public string? CardBackgroundColor { get; init; }
    public string? AccentColor { get; init; }
    public string? HeadingTextColor { get; init; }
    public string? BodyTextColor { get; init; }
    public string? MutedTextColor { get; init; }
}

public sealed class AboutUsSettingsDto
{
    public required string Title { get; init; }
    public string? LeadText { get; init; }
    public required string BodyHtml { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsVisible { get; init; }
    public string? SectionBackgroundColor { get; init; }
    public string? CardBackgroundColor { get; init; }
    public string? AccentColor { get; init; }
    public string? HeadingTextColor { get; init; }
    public string? BodyTextColor { get; init; }
    public string? MutedTextColor { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class AboutUsUpsertDto
{
    public required string Title { get; init; }
    public string? LeadText { get; init; }
    public required string BodyHtml { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsVisible { get; init; }
    public string? SectionBackgroundColor { get; init; }
    public string? CardBackgroundColor { get; init; }
    public string? AccentColor { get; init; }
    public string? HeadingTextColor { get; init; }
    public string? BodyTextColor { get; init; }
    public string? MutedTextColor { get; init; }
}
