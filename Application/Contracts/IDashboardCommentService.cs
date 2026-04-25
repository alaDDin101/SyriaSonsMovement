using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardCommentService
{
    Task<PagedResult<CommentModerationItemDto>> ListAsync(CommentDashboardQuery query, CancellationToken cancellationToken = default);
    Task<bool> SetApprovedAsync(Guid id, bool approved, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
