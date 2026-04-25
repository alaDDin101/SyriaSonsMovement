using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IClientOrganizationService
{
    Task<OrgStructurePageDto?> GetPublicAsync(CancellationToken cancellationToken = default);
}
