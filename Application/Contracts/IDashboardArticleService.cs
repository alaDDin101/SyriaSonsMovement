using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardArticleService
{
    Task<PagedResult<ArticleListItemDto>> ListAsync(ArticleDashboardQuery query, CancellationToken cancellationToken = default);
    Task<ArticleDetailDto?> GetByIdAsync(
        Guid id,
        int commentsPage = 1,
        int commentsPageSize = 20,
        int likesPage = 1,
        int likesPageSize = 20,
        CancellationToken cancellationToken = default);
    Task<ArticleDetailDto> CreateAsync(ArticleUpsertDto dto, Guid? authorId, CancellationToken cancellationToken = default);
    Task<ArticleDetailDto?> UpdateAsync(Guid id, ArticleUpsertDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
