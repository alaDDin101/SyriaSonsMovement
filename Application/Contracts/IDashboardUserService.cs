using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardUserService
{
    Task<PagedResult<UserListItemDto>> ListAsync(UserManagementQuery query, CancellationToken cancellationToken = default);
    Task<UserDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDetailDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<UserDetailDto?> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<UserDetailDto?> SetPasswordAsync(Guid id, AdminSetPasswordDto dto, CancellationToken cancellationToken = default);
    Task<UserDetailDto?> AssignRolesAsync(Guid id, AssignUserRolesDto dto, CancellationToken cancellationToken = default);
    Task<UserDetailDto?> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
