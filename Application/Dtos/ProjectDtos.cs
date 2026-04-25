namespace SyriaSonsMovement.Application.Dtos;

public sealed class ProjectHubCardDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? Summary { get; init; }
    public string? CoverImageUrl { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    /// <summary>Project section as byte (1–3); matches <c>ProjectSection</c> enum.</summary>
    public byte Section { get; init; }
    public required string SectionTitle { get; init; }
}

public sealed class ProjectsHubPageDto
{
    public required string Title { get; init; }
    public string? LeadText { get; init; }
    public string? IntroHtml { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    /// <summary>When set, results are limited to this section; otherwise all sections in display order.</summary>
    public byte? SectionFilter { get; init; }
    public required IReadOnlyList<ProjectHubCardDto> Items { get; init; }
}

public sealed class ProjectPublicDetailDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? Summary { get; init; }
    public required string BodyHtml { get; init; }
    public string? CoverImageUrl { get; init; }
    public byte Section { get; init; }
    public required string SectionTitle { get; init; }
    public bool IsPublished { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public long ViewCount { get; init; }
    public string? AuthorDisplayName { get; init; }
}

public sealed class ProjectsPageSettingsDto
{
    public required string Title { get; init; }
    public string? LeadText { get; init; }
    public string? IntroHtml { get; init; }
    public bool IsVisible { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ProjectsPageSettingsUpsertDto
{
    public required string Title { get; init; }
    public string? LeadText { get; init; }
    public string? IntroHtml { get; init; }
    public bool IsVisible { get; init; }
}

public sealed class ProjectListItemDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? CoverImageUrl { get; init; }
    public byte Section { get; init; }
    public required string SectionName { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsPublished { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public long ViewCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class ProjectDetailDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? Summary { get; init; }
    public required string BodyHtml { get; init; }
    public string? CoverImageUrl { get; init; }
    public byte Section { get; init; }
    public required string SectionName { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsPublished { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public long ViewCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public string? AuthorDisplayName { get; init; }
}

public sealed class ProjectUpsertDto
{
    public byte Section { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? Summary { get; init; }
    public required string BodyHtml { get; init; }
    public string? CoverImageUrl { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsPublished { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
}
