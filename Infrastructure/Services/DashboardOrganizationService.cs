using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Domain.Enums;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardOrganizationService : IDashboardOrganizationService
{
    private readonly ApplicationDbContext _db;

    public DashboardOrganizationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<OrganizationalStructureSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var s = await _db.OrganizationalStructureSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == OrganizationalStructureIds.SettingsSingleton, cancellationToken)
            ?? throw new InvalidOperationException("Organizational structure settings missing; run migrations and seed.");
        return MapSettings(s);
    }

    public async Task<OrganizationalStructureSettingsDto> UpsertSettingsAsync(OrganizationalStructureSettingsUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var s = await _db.OrganizationalStructureSettings.FirstOrDefaultAsync(x => x.Id == OrganizationalStructureIds.SettingsSingleton, cancellationToken);
        if (s == null)
        {
            s = new OrganizationalStructureSettings { Id = OrganizationalStructureIds.SettingsSingleton };
            _db.OrganizationalStructureSettings.Add(s);
        }

        s.Title = dto.Title.Trim();
        s.LeadText = string.IsNullOrWhiteSpace(dto.LeadText) ? null : dto.LeadText.Trim();
        s.IntroHtml = string.IsNullOrWhiteSpace(dto.IntroHtml) ? null : dto.IntroHtml.Trim();
        s.IsVisible = dto.IsVisible;
        s.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return MapSettings(s);
    }

    public async Task<IReadOnlyList<OrgStructureNodeDto>> ListNodesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.OrgStructureNodes.AsNoTracking()
            .OrderBy(n => n.Kind)
            .ThenBy(n => n.ParentId)
            .ThenBy(n => n.DisplayOrder)
            .Select(n => new OrgStructureNodeDto
            {
                Id = n.Id,
                Kind = (byte)n.Kind,
                Name = n.Name,
                Description = n.Description,
                HolderName = n.HolderName,
                ParentId = n.ParentId,
                DisplayOrder = n.DisplayOrder,
                IsActive = n.IsActive,
                UpdatedAt = n.UpdatedAt,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OrgStructureNodeDto> UpsertNodeAsync(OrgStructureNodeUpsertDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Kind is not (0 or 1))
            throw new ArgumentException("Invalid kind.");
        var kind = (OrgStructureNodeKind)dto.Kind;

        if (kind == OrgStructureNodeKind.Committee && dto.ParentId != null)
            throw new ArgumentException("Committees must not have a parent.");
        if (kind == OrgStructureNodeKind.Position && dto.ParentId != null)
        {
            var parent = await _db.OrgStructureNodes.FirstOrDefaultAsync(x => x.Id == dto.ParentId, cancellationToken);
            if (parent == null || parent.Kind != OrgStructureNodeKind.Committee)
                throw new ArgumentException("Position parent must be an existing committee.");
        }

        OrgStructureNode entity;
        if (dto.Id is { } id && id != Guid.Empty)
        {
            entity = await _db.OrgStructureNodes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new InvalidOperationException("Node not found.");
            if (entity.Kind == OrgStructureNodeKind.Committee && kind == OrgStructureNodeKind.Position)
                throw new InvalidOperationException("Cannot change a committee into a position.");
            if (entity.Kind == OrgStructureNodeKind.Position && kind == OrgStructureNodeKind.Committee)
                throw new InvalidOperationException("Cannot change a position into a committee.");
        }
        else
        {
            entity = new OrgStructureNode { Id = Guid.NewGuid() };
            _db.OrgStructureNodes.Add(entity);
        }

        entity.Kind = kind;
        entity.Name = dto.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.HolderName = kind == OrgStructureNodeKind.Position
            ? (string.IsNullOrWhiteSpace(dto.HolderName) ? null : dto.HolderName.Trim())
            : null;
        entity.ParentId = kind == OrgStructureNodeKind.Committee ? null : dto.ParentId;
        entity.DisplayOrder = dto.DisplayOrder;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return new OrgStructureNodeDto
        {
            Id = entity.Id,
            Kind = (byte)entity.Kind,
            Name = entity.Name,
            Description = entity.Description,
            HolderName = entity.HolderName,
            ParentId = entity.ParentId,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    public async Task<bool> DeleteNodeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var n = await _db.OrgStructureNodes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (n == null) return false;
        _db.OrgStructureNodes.Remove(n);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static OrganizationalStructureSettingsDto MapSettings(OrganizationalStructureSettings s) => new()
    {
        Title = s.Title,
        LeadText = s.LeadText,
        IntroHtml = s.IntroHtml,
        IsVisible = s.IsVisible,
        UpdatedAt = s.UpdatedAt,
    };
}
