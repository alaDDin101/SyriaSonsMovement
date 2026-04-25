using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain;
using SyriaSonsMovement.Domain.Enums;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class ClientOrganizationService : IClientOrganizationService
{
    private readonly ApplicationDbContext _db;

    public ClientOrganizationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<OrgStructurePageDto?> GetPublicAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _db.OrganizationalStructureSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == OrganizationalStructureIds.SettingsSingleton, cancellationToken);
        if (settings == null || !settings.IsVisible)
            return null;

        var nodes = await _db.OrgStructureNodes.AsNoTracking()
            .Where(n => n.IsActive)
            .OrderBy(n => n.DisplayOrder)
            .ThenBy(n => n.Name)
            .ToListAsync(cancellationToken);

        var committees = nodes.Where(n => n.Kind == OrgStructureNodeKind.Committee && n.ParentId == null).ToList();
        var byParent = nodes.Where(n => n.ParentId != null).GroupBy(n => n.ParentId!.Value).ToDictionary(g => g.Key, g => g.OrderBy(x => x.DisplayOrder).ToList());

        var committeeDtos = committees.Select(c => new OrgStructureCommitteeDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Positions = (byParent.TryGetValue(c.Id, out var kids)
                    ? kids.Where(x => x.Kind == OrgStructureNodeKind.Position).ToList()
                    : [])
                .Select(p => new OrgStructurePositionDto
                {
                    Id = p.Id,
                    Title = p.Name,
                    HolderName = p.HolderName,
                })
                .ToList(),
        }).ToList();

        var rootPositions = nodes
            .Where(n => n.Kind == OrgStructureNodeKind.Position && n.ParentId == null)
            .OrderBy(n => n.DisplayOrder)
            .Select(p => new OrgStructurePositionDto
            {
                Id = p.Id,
                Title = p.Name,
                HolderName = p.HolderName,
            })
            .ToList();

        return new OrgStructurePageDto
        {
            Title = settings.Title,
            LeadText = settings.LeadText,
            IntroHtml = settings.IntroHtml,
            Committees = committeeDtos,
            RootPositions = rootPositions,
        };
    }
}
