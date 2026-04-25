using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Services;

internal static class CommentMapping
{
    public static IReadOnlyList<ArticleCommentDto> BuildTreeFrom(IReadOnlyList<ArticleComment> comments, Guid? parentId)
    {
        return comments
            .Where(c => c.ParentCommentId == parentId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new ArticleCommentDto
            {
                Id = c.Id,
                Body = c.Body,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                UserDisplayName = c.User?.DisplayName ?? c.User?.Email,
                ParentCommentId = c.ParentCommentId,
                IsApproved = c.IsApproved,
                Replies = BuildTreeFrom(comments, c.Id).ToList(),
            })
            .ToList();
    }
}
