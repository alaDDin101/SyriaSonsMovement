using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardCommentService : IDashboardCommentService
{
    private readonly ApplicationDbContext _db;

    public DashboardCommentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<CommentModerationItemDto>> ListAsync(CommentDashboardQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = _db.ArticleComments.AsNoTracking();
        if (query.ArticleId.HasValue)
            q = q.Where(c => c.ArticleId == query.ArticleId.Value);
        if (query.ApprovedOnly.HasValue)
            q = q.Where(c => c.IsApproved == query.ApprovedOnly.Value);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CommentModerationItemDto
            {
                Id = c.Id,
                ArticleId = c.ArticleId,
                ArticleTitle = c.Article.Title,
                Body = c.Body,
                IsApproved = c.IsApproved,
                IsDeleted = c.IsDeleted,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                UserEmail = c.User.Email,
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CommentModerationItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<bool> SetApprovedAsync(Guid id, bool approved, CancellationToken cancellationToken = default)
    {
        var n = await _db.ArticleComments
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(sp => sp.SetProperty(c => c.IsApproved, approved), cancellationToken);
        return n > 0;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var n = await _db.ArticleComments
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(sp => sp.SetProperty(c => c.IsDeleted, true), cancellationToken);
        return n > 0;
    }
}
