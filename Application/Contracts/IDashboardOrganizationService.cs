using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardOrganizationService
{
    Task<OrganizationalStructureSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<OrganizationalStructureSettingsDto> UpsertSettingsAsync(OrganizationalStructureSettingsUpsertDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrgStructureNodeDto>> ListNodesAsync(CancellationToken cancellationToken = default);
    Task<OrgStructureNodeDto> UpsertNodeAsync(OrgStructureNodeUpsertDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteNodeAsync(Guid id, CancellationToken cancellationToken = default);
}
