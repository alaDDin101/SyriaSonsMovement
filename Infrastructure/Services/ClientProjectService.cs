using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain;
using SyriaSonsMovement.Domain.Enums;
using SyriaSonsMovement.Infrastructure.Persistence;
using SyriaSonsMovement.Infrastructure.Utilities;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class ClientProjectService : IClientProjectService
{
    private const int DefaultHubPageSize = 12;
    private const int MaxHubPageSize = 100;

    private readonly ApplicationDbContext _db;

    public ClientProjectService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectsHubPageDto?> GetHubAsync(
        int page,
        int pageSize,
        byte? section,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await _db.ProjectsPageSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == ProjectsPageIds.SettingsSingleton, cancellationToken);
        if (settings == null || !settings.IsVisible)
            return null;

        var p = page < 1 ? 1 : page;
        var size = pageSize < 1 ? DefaultHubPageSize : Math.Min(pageSize, MaxHubPageSize);

        var q = _db.Projects.AsNoTracking()
            .Where(pr => pr.IsPublished && pr.PublishedAt != null);

        byte? sectionFilter = null;
        if (section is >= 1 and <= 3 && Enum.IsDefined(typeof(ProjectSection), section.Value))
        {
            var sec = (ProjectSection)section.Value;
            q = q.Where(pr => pr.Section == sec);
            sectionFilter = section;
        }

        var trimmedSearch = search?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedSearch))
        {
            q = q.Where(pr =>
                EF.Functions.Like(pr.Title, $"%{trimmedSearch}%")
                || (pr.Summary != null && EF.Functions.Like(pr.Summary, $"%{trimmedSearch}%"))
                || EF.Functions.Like(pr.BodyHtml, $"%{trimmedSearch}%"));
        }

        var ordered = q
            .OrderBy(pr => pr.Section)
            .ThenBy(pr => pr.DisplayOrder)
            .ThenByDescending(pr => pr.PublishedAt);

        var totalCount = await ordered.CountAsync(cancellationToken);

        var rows = await ordered
            .Skip((p - 1) * size)
            .Take(size)
            .Select(pr => new
            {
                pr.Id,
                pr.Title,
                pr.Slug,
                pr.Summary,
                pr.CoverImageUrl,
                pr.PublishedAt,
                pr.Section,
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(pr => new ProjectHubCardDto
        {
            Id = pr.Id,
            Title = pr.Title,
            Slug = pr.Slug,
            Summary = pr.Summary,
            CoverImageUrl = pr.CoverImageUrl,
            PublishedAt = pr.PublishedAt,
            Section = (byte)pr.Section,
            SectionTitle = ProjectSectionLabels.Arabic(pr.Section),
        }).ToList();

        return new ProjectsHubPageDto
        {
            Title = settings.Title,
            LeadText = settings.LeadText,
            IntroHtml = settings.IntroHtml,
            Page = p,
            PageSize = size,
            TotalCount = totalCount,
            SectionFilter = sectionFilter,
            Items = items,
        };
    }

    public async Task<ProjectPublicDetailDto?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var s = slug.Trim().ToLowerInvariant();
        var project = await _db.Projects.AsNoTracking()
            .Include(pr => pr.Author)
            .FirstOrDefaultAsync(pr => pr.Slug == s && pr.IsPublished && pr.PublishedAt != null, cancellationToken);

        if (project == null)
            return null;

        await _db.Projects
            .Where(pr => pr.Id == project.Id)
            .ExecuteUpdateAsync(sp => sp.SetProperty(pr => pr.ViewCount, pr => pr.ViewCount + 1), cancellationToken);

        var section = project.Section;
        return new ProjectPublicDetailDto
        {
            Id = project.Id,
            Title = project.Title,
            Slug = project.Slug,
            Summary = project.Summary,
            BodyHtml = project.BodyHtml,
            CoverImageUrl = project.CoverImageUrl,
            Section = (byte)section,
            SectionTitle = ProjectSectionLabels.Arabic(section),
            IsPublished = project.IsPublished,
            PublishedAt = project.PublishedAt,
            ViewCount = project.ViewCount + 1,
            AuthorDisplayName = project.Author?.DisplayName ?? project.Author?.Email,
        };
    }
}
