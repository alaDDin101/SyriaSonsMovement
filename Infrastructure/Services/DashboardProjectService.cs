using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Domain.Enums;
using SyriaSonsMovement.Infrastructure.Persistence;
using SyriaSonsMovement.Infrastructure.Utilities;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardProjectService : IDashboardProjectService
{
    private readonly ApplicationDbContext _db;

    public DashboardProjectService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectsPageSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var s = await _db.ProjectsPageSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == ProjectsPageIds.SettingsSingleton, cancellationToken)
            ?? throw new InvalidOperationException("Projects page settings missing; run migrations and seed.");
        return MapSettings(s);
    }

    public async Task<ProjectsPageSettingsDto> UpsertSettingsAsync(ProjectsPageSettingsUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var s = await _db.ProjectsPageSettings.FirstOrDefaultAsync(x => x.Id == ProjectsPageIds.SettingsSingleton, cancellationToken);
        if (s == null)
        {
            s = new ProjectsPageSettings { Id = ProjectsPageIds.SettingsSingleton };
            _db.ProjectsPageSettings.Add(s);
        }

        s.Title = dto.Title.Trim();
        s.LeadText = string.IsNullOrWhiteSpace(dto.LeadText) ? null : dto.LeadText.Trim();
        s.IntroHtml = string.IsNullOrWhiteSpace(dto.IntroHtml) ? null : dto.IntroHtml.Trim();
        s.IsVisible = dto.IsVisible;
        s.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return MapSettings(s);
    }

    public async Task<PagedResult<ProjectListItemDto>> ListAsync(ProjectDashboardQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = _db.Projects.AsNoTracking();
        if (query.Section is { } sec && ProjectSectionLabels.IsDefined(sec))
            q = q.Where(p => (byte)p.Section == sec);
        if (query.IsPublished.HasValue)
            q = q.Where(p => p.IsPublished == query.IsPublished.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(p => p.Title.Contains(s) || p.Slug.Contains(s));
        }

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectListItemDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                CoverImageUrl = p.CoverImageUrl,
                Section = (byte)p.Section,
                SectionName = p.Section == ProjectSection.WhatWeOffer
                    ? "ماذا تقدم الحركة عملياً"
                    : p.Section == ProjectSection.ServiceOrAwareness
                        ? "مشاريع خدمية أو توعوية"
                        : "خطط مستقبلية",
                DisplayOrder = p.DisplayOrder,
                IsPublished = p.IsPublished,
                PublishedAt = p.PublishedAt,
                ViewCount = p.ViewCount,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ProjectDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var p = await _db.Projects.AsNoTracking()
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return p == null ? null : MapDetail(p);
    }

    public async Task<ProjectDetailDto> CreateAsync(ProjectUpsertDto dto, Guid? authorId, CancellationToken cancellationToken = default)
    {
        if (!ProjectSectionLabels.IsDefined(dto.Section))
            throw new InvalidOperationException("Invalid section.");

        var slug = SlugHelper.Normalize(dto.Slug);
        if (await _db.Projects.AnyAsync(p => p.Slug == slug, cancellationToken))
            throw new InvalidOperationException("Slug already in use.");

        var now = DateTimeOffset.UtcNow;
        var publishedAt = dto.PublishedAt;
        if (dto.IsPublished && publishedAt == null)
            publishedAt = now;

        var entity = new Project
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            Section = (ProjectSection)dto.Section,
            Title = dto.Title.Trim(),
            Slug = slug,
            Summary = string.IsNullOrWhiteSpace(dto.Summary) ? null : dto.Summary.Trim(),
            BodyHtml = dto.BodyHtml,
            CoverImageUrl = string.IsNullOrWhiteSpace(dto.CoverImageUrl) ? null : dto.CoverImageUrl.Trim(),
            DisplayOrder = dto.DisplayOrder,
            IsPublished = dto.IsPublished,
            PublishedAt = publishedAt,
            ViewCount = 0,
            CreatedAt = now,
        };

        _db.Projects.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<ProjectDetailDto?> UpdateAsync(Guid id, ProjectUpsertDto dto, CancellationToken cancellationToken = default)
    {
        if (!ProjectSectionLabels.IsDefined(dto.Section))
            throw new InvalidOperationException("Invalid section.");

        var entity = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity == null)
            return null;

        var slug = SlugHelper.Normalize(dto.Slug);
        if (await _db.Projects.AnyAsync(p => p.Slug == slug && p.Id != id, cancellationToken))
            throw new InvalidOperationException("Slug already in use.");

        var now = DateTimeOffset.UtcNow;
        entity.Section = (ProjectSection)dto.Section;
        entity.Title = dto.Title.Trim();
        entity.Slug = slug;
        entity.Summary = string.IsNullOrWhiteSpace(dto.Summary) ? null : dto.Summary.Trim();
        entity.BodyHtml = dto.BodyHtml;
        entity.CoverImageUrl = string.IsNullOrWhiteSpace(dto.CoverImageUrl) ? null : dto.CoverImageUrl.Trim();
        entity.DisplayOrder = dto.DisplayOrder;
        entity.IsPublished = dto.IsPublished;
        if (dto.IsPublished)
        {
            var publishedAt = dto.PublishedAt;
            if (publishedAt == null && entity.PublishedAt == null)
                publishedAt = now;
            entity.PublishedAt = publishedAt ?? entity.PublishedAt;
        }
        else
        {
            entity.PublishedAt = null;
        }

        entity.UpdatedAt = now;
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var n = await _db.Projects.Where(p => p.Id == id).ExecuteDeleteAsync(cancellationToken);
        return n > 0;
    }

    private static ProjectsPageSettingsDto MapSettings(ProjectsPageSettings s) => new()
    {
        Title = s.Title,
        LeadText = s.LeadText,
        IntroHtml = s.IntroHtml,
        IsVisible = s.IsVisible,
        UpdatedAt = s.UpdatedAt,
    };

    private static ProjectDetailDto MapDetail(Project p)
    {
        var sec = p.Section;
        return new ProjectDetailDto
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Summary = p.Summary,
            BodyHtml = p.BodyHtml,
            CoverImageUrl = p.CoverImageUrl,
            Section = (byte)sec,
            SectionName = ProjectSectionLabels.Arabic(sec),
            DisplayOrder = p.DisplayOrder,
            IsPublished = p.IsPublished,
            PublishedAt = p.PublishedAt,
            ViewCount = p.ViewCount,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            AuthorDisplayName = p.Author?.DisplayName ?? p.Author?.Email,
        };
    }
}
