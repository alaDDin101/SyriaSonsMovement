using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardPermissionService
{
    Task<IReadOnlyList<PermissionDto>> ListAsync(CancellationToken cancellationToken = default);
}
