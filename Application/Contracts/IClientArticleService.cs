using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IClientArticleService
{
    Task<PagedResult<ArticleSummaryDto>> ListPublishedAsync(
        int page,
        int pageSize,
        Guid? categoryId,
        string? search = null,
        CancellationToken cancellationToken = default);
    Task<ArticleDetailDto?> GetBySlugAsync(
        string slug,
        Guid? currentUserId,
        string? viewerIp,
        int commentsPage = 1,
        int commentsPageSize = 20,
        CancellationToken cancellationToken = default);
    Task LikeAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default);
    Task UnlikeAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default);
    Task<ArticleCommentDto> AddCommentAsync(Guid articleId, Guid userId, ArticleCommentCreateDto dto, CancellationToken cancellationToken = default);
    Task DeleteCommentAsync(Guid articleId, Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}
