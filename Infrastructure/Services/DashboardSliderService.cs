using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Domain.Enums;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardSliderService : IDashboardSliderService
{
    private readonly ApplicationDbContext _db;

    public DashboardSliderService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ArticleTitleSearchItemDto>> SearchArticlesByTitleAsync(
        string title,
        int take,
        CancellationToken cancellationToken = default)
    {
        var t = title.Trim().ToLowerInvariant();
        var limit = Math.Clamp(take, 1, 50);
        return await _db.Articles
            .AsNoTracking()
            .Where(a => a.Title.ToLower().Contains(t))
            .OrderByDescending(a => a.IsPublished)
            .ThenByDescending(a => a.CreatedAt)
            .Take(limit)
            .Select(a => new ArticleTitleSearchItemDto
            {
                Id = a.Id,
                Title = a.Title,
                Slug = a.Slug,
                IsPublished = a.IsPublished,
                Summary = a.Summary,
                CoverImageUrl = a.CoverImageUrl,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SliderSlideDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.HomeSliderSlides
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .Include(s => s.Article)
            .Select(s => new SliderSlideDto
            {
                Id = s.Id,
                BackgroundImageUrl = s.BackgroundImageUrl,
                Title = s.Title,
                Subtitle = s.Subtitle,
                ContentHtml = s.ContentHtml,
                LinkTargetType = s.LinkTargetType,
                ArticleId = s.ArticleId,
                ArticleTitle = s.Article != null ? s.Article.Title : null,
                ArticleSlug = s.Article != null ? s.Article.Slug : null,
                ExternalUrl = s.ExternalUrl,
                OpenInNewTab = s.OpenInNewTab,
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
    }

    public async Task<SliderSlideDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.HomeSliderSlides
            .AsNoTracking()
            .Include(s => s.Article)
            .Where(s => s.Id == id)
            .Select(s => new SliderSlideDto
            {
                Id = s.Id,
                BackgroundImageUrl = s.BackgroundImageUrl,
                Title = s.Title,
                Subtitle = s.Subtitle,
                ContentHtml = s.ContentHtml,
                LinkTargetType = s.LinkTargetType,
                ArticleId = s.ArticleId,
                ArticleTitle = s.Article != null ? s.Article.Title : null,
                ArticleSlug = s.Article != null ? s.Article.Slug : null,
                ExternalUrl = s.ExternalUrl,
                OpenInNewTab = s.OpenInNewTab,
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
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SliderSlideDto> CreateAsync(SliderSlideUpsertDto dto, CancellationToken cancellationToken = default)
    {
        await ValidateLinkAsync(dto, cancellationToken);

        var entity = new HomeSliderSlide
        {
            Id = Guid.NewGuid(),
            BackgroundImageUrl = dto.BackgroundImageUrl,
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            ContentHtml = dto.ContentHtml,
            LinkTargetType = dto.LinkTargetType,
            ArticleId = dto.LinkTargetType == SliderLinkTargetType.InternalArticle ? dto.ArticleId : null,
            ExternalUrl = dto.LinkTargetType == SliderLinkTargetType.ExternalUrl ? dto.ExternalUrl : null,
            OpenInNewTab = dto.OpenInNewTab,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        ApplyStyleFromUpsert(entity, dto);
        _db.HomeSliderSlides.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<SliderSlideDto?> UpdateAsync(Guid id, SliderSlideUpsertDto dto, CancellationToken cancellationToken = default)
    {
        await ValidateLinkAsync(dto, cancellationToken);

        var entity = await _db.HomeSliderSlides.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (entity == null)
            return null;

        entity.BackgroundImageUrl = dto.BackgroundImageUrl;
        entity.Title = dto.Title;
        entity.Subtitle = dto.Subtitle;
        entity.ContentHtml = dto.ContentHtml;
        entity.LinkTargetType = dto.LinkTargetType;
        entity.ArticleId = dto.LinkTargetType == SliderLinkTargetType.InternalArticle ? dto.ArticleId : null;
        entity.ExternalUrl = dto.LinkTargetType == SliderLinkTargetType.ExternalUrl ? dto.ExternalUrl : null;
        entity.OpenInNewTab = dto.OpenInNewTab;
        entity.DisplayOrder = dto.DisplayOrder;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        ApplyStyleFromUpsert(entity, dto);
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var n = await _db.HomeSliderSlides.Where(s => s.Id == id).ExecuteDeleteAsync(cancellationToken);
        return n > 0;
    }

    private static void ApplyStyleFromUpsert(HomeSliderSlide e, SliderSlideUpsertDto dto)
    {
        e.TitleColor = NullIfWhiteSpace(dto.TitleColor);
        e.SubtitleTextColor = NullIfWhiteSpace(dto.SubtitleTextColor);
        e.SubtitleBadgeBackgroundColor = NullIfWhiteSpace(dto.SubtitleBadgeBackgroundColor);
        e.SubtitleBadgeBorderColor = NullIfWhiteSpace(dto.SubtitleBadgeBorderColor);
        e.ContentHtmlColor = NullIfWhiteSpace(dto.ContentHtmlColor);
        e.CtaBackgroundColor = NullIfWhiteSpace(dto.CtaBackgroundColor);
        e.CtaTextColor = NullIfWhiteSpace(dto.CtaTextColor);
        e.NavArrowBackgroundColor = NullIfWhiteSpace(dto.NavArrowBackgroundColor);
        e.NavArrowIconColor = NullIfWhiteSpace(dto.NavArrowIconColor);
        e.DotActiveColor = NullIfWhiteSpace(dto.DotActiveColor);
        e.DotInactiveColor = NullIfWhiteSpace(dto.DotInactiveColor);
        e.OverlayBottomColor = NullIfWhiteSpace(dto.OverlayBottomColor);
        e.OverlayMiddleColor = NullIfWhiteSpace(dto.OverlayMiddleColor);
        e.OverlayTopColor = NullIfWhiteSpace(dto.OverlayTopColor);
    }

    private static string? NullIfWhiteSpace(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private async Task ValidateLinkAsync(SliderSlideUpsertDto dto, CancellationToken cancellationToken)
    {
        switch (dto.LinkTargetType)
        {
            case SliderLinkTargetType.None:
                break;
            case SliderLinkTargetType.ExternalUrl:
                if (string.IsNullOrWhiteSpace(dto.ExternalUrl))
                    throw new InvalidOperationException("ExternalUrl is required for external links.");
                break;
            case SliderLinkTargetType.InternalArticle:
                if (!dto.ArticleId.HasValue)
                    throw new InvalidOperationException("ArticleId is required for article links.");
                if (!await _db.Articles.AnyAsync(a => a.Id == dto.ArticleId.Value, cancellationToken))
                    throw new InvalidOperationException("Article not found.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dto.LinkTargetType), dto.LinkTargetType, null);
        }
    }
}
