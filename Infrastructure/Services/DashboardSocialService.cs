using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardSocialService : IDashboardSocialService
{
    private readonly ApplicationDbContext _db;

    public DashboardSocialService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SocialLinkDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SocialMediaLinks
            .AsNoTracking()
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
    }

    public async Task<SocialLinkDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.SocialMediaLinks
            .AsNoTracking()
            .Where(l => l.Id == id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SocialLinkDto> CreateAsync(SocialLinkUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new SocialMediaLink
        {
            Id = Guid.NewGuid(),
            PlatformKey = dto.PlatformKey.Trim(),
            Label = dto.Label,
            Url = dto.Url.Trim(),
            IconUrl = dto.IconUrl,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
        };
        _db.SocialMediaLinks.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<SocialLinkDto?> UpdateAsync(Guid id, SocialLinkUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.SocialMediaLinks.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (entity == null)
            return null;

        entity.PlatformKey = dto.PlatformKey.Trim();
        entity.Label = dto.Label;
        entity.Url = dto.Url.Trim();
        entity.IconUrl = dto.IconUrl;
        entity.DisplayOrder = dto.DisplayOrder;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var n = await _db.SocialMediaLinks.Where(l => l.Id == id).ExecuteDeleteAsync(cancellationToken);
        return n > 0;
    }
}
