using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardMembershipService
{
    Task<PagedResult<MembershipJoinRequestListItemDto>> ListPendingAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<bool> ApproveAsync(Guid userId, Guid reviewedByUserId, CancellationToken cancellationToken = default);
    Task<bool> RejectAsync(Guid userId, Guid reviewedByUserId, CancellationToken cancellationToken = default);
    Task<MembershipJoinRequestListItemDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
