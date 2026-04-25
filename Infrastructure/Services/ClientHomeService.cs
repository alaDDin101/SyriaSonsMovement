using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain;
using SyriaSonsMovement.Domain.Enums;
using SyriaSonsMovement.Infrastructure.Persistence;
using SyriaSonsMovement.Infrastructure.Utilities;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class ClientHomeService : IClientHomeService
{
    private const int DefaultHomeArticlesPageSize = 10;
    private const int DefaultHomeProjectsPageSize = 6;
    private const int MaxHomeProjectsPageSize = 20;

    private readonly ApplicationDbContext _db;
    private readonly IClientArticleService _articles;

    public ClientHomeService(ApplicationDbContext db, IClientArticleService articles)
    {
        _db = db;
        _articles = articles;
    }

    public async Task<HomePageDto> GetHomeAsync(CancellationToken cancellationToken = default)
    {
        var slides = await _db.HomeSliderSlides
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Include(s => s.Article)
            .Select(s => new SliderSlideDto
            {
                Id = s.Id,
                BackgroundImageUrl = s.BackgroundImageUrl,
                Title = s.Title,
                Subtitle = s.Subtitle,
                ContentHtml = s.ContentHtml,
                LinkTargetType =
                    s.LinkTargetType == SliderLinkTargetType.InternalArticle && s.Article != null && !s.Article.IsPublished
                        ? SliderLinkTargetType.None
                        : s.LinkTargetType,
                ArticleId =
                    s.LinkTargetType == SliderLinkTargetType.InternalArticle && s.Article != null && !s.Article.IsPublished
                        ? null
                        : s.ArticleId,
                ArticleTitle =
                    s.LinkTargetType == SliderLinkTargetType.InternalArticle && s.Article != null && !s.Article.IsPublished
                        ? null
                        : (s.Article != null ? s.Article.Title : null),
                ArticleSlug =
                    s.LinkTargetType == SliderLinkTargetType.InternalArticle && s.Article != null && !s.Article.IsPublished
                        ? null
                        : (s.Article != null ? s.Article.Slug : null),
                ExternalUrl =
                    s.LinkTargetType == SliderLinkTargetType.InternalArticle && s.Article != null && !s.Article.IsPublished
                        ? null
                        : s.ExternalUrl,
                OpenInNewTab =
                    s.LinkTargetType == SliderLinkTargetType.InternalArticle && s.Article != null && !s.Article.IsPublished
                        ? false
                        : s.OpenInNewTab,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                TitleColor = s.TitleColor,
                SubtitleTextColor = s.SubtitleTextColor,
                SubtitleBadgeBackgroundColor = s.SubtitleBadgeBackgroundColor,
                SubtitleBadgeBorderColor = s.SubtitleBadgeBorderColor,
                ContentHtmlColor = s.ContentHtmlColor,
                CtaBackgroundColor = s.CtaBackgroundColor,
                CtaTextColor = s.CtaTextColor,
                NavArrowBackgroundColor = s.NavArrowBackgroundColor,
                NavArrowIconColor = s.NavArrowIconColor,
                DotActiveColor = s.DotActiveColor,
                DotInactiveColor = s.DotInactiveColor,
                OverlayBottomColor = s.OverlayBottomColor,
                OverlayMiddleColor = s.OverlayMiddleColor,
                OverlayTopColor = s.OverlayTopColor,
            })
            .ToListAsync(cancellationToken);

        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryCardDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                BackgroundImageUrl = c.BackgroundImageUrl,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
            })
            .ToListAsync(cancellationToken);

        var articlesSection = await _articles.ListPublishedAsync(
            1,
            DefaultHomeArticlesPageSize,
            null,
            null,
            cancellationToken);

        var social = await _db.SocialMediaLinks
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.DisplayOrder)
            .Select(l => new SocialLinkDto
            {
                Id = l.Id,
                PlatformKey = l.PlatformKey,
                Label = l.Label,
                Url = l.Url,
                IconUrl = l.IconUrl,
                DisplayOrder = l.DisplayOrder,
                IsActive = l.IsActive,
            })
            .ToListAsync(cancellationToken);

        var about = await _db.AboutUsSettings
            .AsNoTracking()
            .Where(a => a.Id == AboutUsIds.Singleton && a.IsVisible)
            .Select(a => new AboutUsPublicDto
            {
                Title = a.Title,
                LeadText = a.LeadText,
                BodyHtml = a.BodyHtml,
                ImageUrl = a.ImageUrl,
                SectionBackgroundColor = a.SectionBackgroundColor,
                CardBackgroundColor = a.CardBackgroundColor,
                AccentColor = a.AccentColor,
                HeadingTextColor = a.HeadingTextColor,
                BodyTextColor = a.BodyTextColor,
                MutedTextColor = a.MutedTextColor,
            })
            .FirstOrDefaultAsync(cancellationToken);

        var projectsSection = await GetHomeProjectsPageAsync(1, DefaultHomeProjectsPageSize, cancellationToken);

        return new HomePageDto
        {
            Slides = slides,
            Categories = categories,
            ArticlesSection = articlesSection,
            SocialLinks = social,
            AboutUs = about,
            ProjectsSection = projectsSection,
        };
    }

    public async Task<HomeProjectsSectionDto?> GetHomeProjectsPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var settings = await _db.ProjectsPageSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == ProjectsPageIds.SettingsSingleton, cancellationToken);
        if (settings == null || !settings.IsVisible)
            return null;

        var p = page < 1 ? 1 : page;
        var size = pageSize < 1 ? DefaultHomeProjectsPageSize : Math.Min(pageSize, MaxHomeProjectsPageSize);

        var baseQuery = _db.Projects.AsNoTracking()
            .Where(x => x.IsPublished && x.PublishedAt != null)
            .OrderBy(x => x.Section)
            .ThenBy(x => x.DisplayOrder)
            .ThenByDescending(x => x.PublishedAt);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var rows = await baseQuery
            .Skip((p - 1) * size)
            .Take(size)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.Summary,
                x.CoverImageUrl,
                x.PublishedAt,
                x.Section,
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(x => new HomeProjectSlideDto
        {
            Id = x.Id,
            Title = x.Title,
            Slug = x.Slug,
            Summary = x.Summary,
            CoverImageUrl = x.CoverImageUrl,
            PublishedAt = x.PublishedAt,
            SectionTitle = ProjectSectionLabels.Arabic(x.Section),
        }).ToList();

        return new HomeProjectsSectionDto
        {
            Title = settings.Title,
            LeadText = settings.LeadText,
            Page = p,
            PageSize = size,
            TotalCount = totalCount,
            Items = items,
        };
    }
}
