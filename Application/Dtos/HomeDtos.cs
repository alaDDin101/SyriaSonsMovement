using SyriaSonsMovement.Application.Common;

namespace SyriaSonsMovement.Application.Dtos;

public sealed class HomeProjectSlideDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? Summary { get; init; }
    public string? CoverImageUrl { get; init; }
    public required string SectionTitle { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
}

/// <summary>Projects/initiatives strip for the home page (when the hub page is visible).</summary>
public sealed class HomeProjectsSectionDto
{
    public required string Title { get; init; }
    public string? LeadText { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public required IReadOnlyList<HomeProjectSlideDto> Items { get; init; }
}

public sealed class HomePageDto
{
    public required IReadOnlyList<SliderSlideDto> Slides { get; init; }
    public required IReadOnlyList<CategoryCardDto> Categories { get; init; }
    /// <summary>First page of published articles for the home horizontal strip (DB-paged; use <c>GET /articles</c> for more pages).</summary>
    public required PagedResult<ArticleSummaryDto> ArticlesSection { get; init; }
    public required IReadOnlyList<SocialLinkDto> SocialLinks { get; init; }
    /// <summary>Present when the block is enabled for the public site.</summary>
    public AboutUsPublicDto? AboutUs { get; init; }
    /// <summary>First page of published projects when «المشاريع والمبادرات» is visible.</summary>
    public HomeProjectsSectionDto? ProjectsSection { get; init; }
}
