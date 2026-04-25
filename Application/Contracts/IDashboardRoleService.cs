using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardRoleService
{
    Task<IReadOnlyList<RoleListItemDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<RoleDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<RoleDetailDto?> UpdateAsync(Guid id, UpdateRoleDto dto, CancellationToken cancellationToken = default);
    Task<RoleDetailDto?> SetPermissionsAsync(Guid id, SetRolePermissionsDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
