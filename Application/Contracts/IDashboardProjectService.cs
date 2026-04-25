using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardProjectService
{
    Task<ProjectsPageSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<ProjectsPageSettingsDto> UpsertSettingsAsync(ProjectsPageSettingsUpsertDto dto, CancellationToken cancellationToken = default);
    Task<PagedResult<ProjectListItemDto>> ListAsync(ProjectDashboardQuery query, CancellationToken cancellationToken = default);
    Task<ProjectDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProjectDetailDto> CreateAsync(ProjectUpsertDto dto, Guid? authorId, CancellationToken cancellationToken = default);
    Task<ProjectDetailDto?> UpdateAsync(Guid id, ProjectUpsertDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
