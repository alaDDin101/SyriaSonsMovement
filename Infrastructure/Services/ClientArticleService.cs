using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class ClientArticleService : IClientArticleService
{
    private readonly ApplicationDbContext _db;

    public ClientArticleService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ArticleSummaryDto>> ListPublishedAsync(
        int page,
        int pageSize,
        Guid? categoryId,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Articles
            .AsNoTracking()
            .Where(a => a.IsPublished && a.PublishedAt != null);

        if (categoryId.HasValue)
            q = q.Where(a => a.CategoryId == categoryId.Value);

        var trimmedSearch = search?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedSearch))
        {
            q = q.Where(a =>
                EF.Functions.Like(a.Title, $"%{trimmedSearch}%")
                || (a.Summary != null && EF.Functions.Like(a.Summary, $"%{trimmedSearch}%"))
                || EF.Functions.Like(a.Category.Name, $"%{trimmedSearch}%"));
        }

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(a => a.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleSummaryDto
            {
                Id = a.Id,
                Title = a.Title,
                Slug = a.Slug,
                Summary = a.Summary,
                CoverImageUrl = a.CoverImageUrl,
                PublishedAt = a.PublishedAt,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                ViewCount = a.ViewCount,
                LikeCount = a.Likes.Count,
                CommentCount = a.Comments.Count(c => c.IsApproved && !c.IsDeleted),
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleSummaryDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ArticleDetailDto?> GetBySlugAsync(
        string slug,
        Guid? currentUserId,
        string? viewerIp,
        int commentsPage = 1,
        int commentsPageSize = 20,
        CancellationToken cancellationToken = default)
    {
        commentsPage = Math.Max(1, commentsPage);
        commentsPageSize = Math.Clamp(commentsPageSize, 1, 100);

        var s = slug.Trim().ToLowerInvariant();
        var article = await _db.Articles
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Slug == s && a.IsPublished, cancellationToken);

        if (article == null)
            return null;

        var didCountView = await TryCountUniqueViewAsync(article.Id, viewerIp, cancellationToken);
        if (didCountView)
        {
            await _db.Articles
                .Where(a => a.Id == article.Id)
                .ExecuteUpdateAsync(sp => sp.SetProperty(a => a.ViewCount, a => a.ViewCount + 1), cancellationToken);
        }

        var likeCount = await _db.ArticleLikes.CountAsync(l => l.ArticleId == article.Id, cancellationToken);
        var liked = currentUserId.HasValue
            && await _db.ArticleLikes.AnyAsync(
                l => l.ArticleId == article.Id && l.UserId == currentUserId.Value,
                cancellationToken);

        var topLevelCommentsBaseQuery = _db.ArticleComments
            .AsNoTracking()
            .Where(c =>
                c.ArticleId == article.Id
                && !c.IsDeleted
                && c.IsApproved
                && c.ParentCommentId == null);

        var totalTopLevelComments = await topLevelCommentsBaseQuery.CountAsync(cancellationToken);

        var topLevelIds = await topLevelCommentsBaseQuery
            .OrderBy(c => c.CreatedAt)
            .Skip((commentsPage - 1) * commentsPageSize)
            .Take(commentsPageSize)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var commentsForPage = new List<ArticleComment>();
        if (topLevelIds.Count > 0)
        {
            var visitedIds = new HashSet<Guid>();
            var frontier = topLevelIds;

            while (frontier.Count > 0)
            {
                var currentLevel = await _db.ArticleComments
                    .AsNoTracking()
                    .Include(c => c.User)
                    .Where(c =>
                        c.ArticleId == article.Id
                        && !c.IsDeleted
                        && c.IsApproved
                        && (frontier.Contains(c.Id)
                            || (c.ParentCommentId.HasValue && frontier.Contains(c.ParentCommentId.Value))))
                    .ToListAsync(cancellationToken);

                var newNodes = currentLevel
                    .Where(c => visitedIds.Add(c.Id))
                    .ToList();

                commentsForPage.AddRange(newNodes);
                frontier = newNodes.Select(c => c.Id).ToList();
            }
        }

        var tree = CommentMapping.BuildTreeFrom(commentsForPage, null);

        return new ArticleDetailDto
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            BodyHtml = article.BodyHtml,
            CoverImageUrl = article.CoverImageUrl,
            IsPublished = article.IsPublished,
            PublishedAt = article.PublishedAt,
            ViewCount = article.ViewCount + (didCountView ? 1 : 0),
            LikeCount = likeCount,
            CategoryId = article.CategoryId,
            CategoryName = article.Category.Name,
            AuthorDisplayName = article.Author?.DisplayName ?? article.Author?.Email,
            LikedByCurrentUser = liked,
            Comments = new PagedResult<ArticleCommentDto>
            {
                Items = tree,
                TotalCount = totalTopLevelComments,
                Page = commentsPage,
                PageSize = commentsPageSize,
            },
        };
    }

    public async Task LikeAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default)
    {
        var articleExists = await _db.Articles.AnyAsync(
            a => a.Id == articleId && a.IsPublished,
            cancellationToken);
        if (!articleExists)
            throw new InvalidOperationException("Article not found.");

        if (await _db.ArticleLikes.AnyAsync(l => l.ArticleId == articleId && l.UserId == userId, cancellationToken))
            return;

        _db.ArticleLikes.Add(new ArticleLike
        {
            ArticleId = articleId,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlikeAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _db.ArticleLikes
            .Where(l => l.ArticleId == articleId && l.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<ArticleCommentDto> AddCommentAsync(
        Guid articleId,
        Guid userId,
        ArticleCommentCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var articleExists = await _db.Articles.AnyAsync(
            a => a.Id == articleId && a.IsPublished,
            cancellationToken);
        if (!articleExists)
            throw new InvalidOperationException("Article not found.");

        if (dto.ParentCommentId.HasValue)
        {
            var parentOk = await _db.ArticleComments.AnyAsync(
                c => c.Id == dto.ParentCommentId.Value && c.ArticleId == articleId && !c.IsDeleted,
                cancellationToken);
            if (!parentOk)
                throw new InvalidOperationException("Parent comment not found.");
        }

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        var entity = new ArticleComment
        {
            Id = Guid.NewGuid(),
            ArticleId = articleId,
            UserId = userId,
            ParentCommentId = dto.ParentCommentId,
            Body = dto.Body.Trim(),
            IsDeleted = false,
            IsApproved = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.ArticleComments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new ArticleCommentDto
        {
            Id = entity.Id,
            Body = entity.Body,
            CreatedAt = entity.CreatedAt,
            UserId = userId,
            UserDisplayName = user.DisplayName ?? user.Email,
            ParentCommentId = entity.ParentCommentId,
            IsApproved = entity.IsApproved,
            Replies = [],
        };
    }

    public async Task DeleteCommentAsync(Guid articleId, Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var root = await _db.ArticleComments
            .AsNoTracking()
            .Where(c => c.Id == commentId && c.ArticleId == articleId && !c.IsDeleted)
            .Select(c => new { c.Id, c.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (root == null)
            throw new InvalidOperationException("Comment not found.");
        if (root.UserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        var ids = new HashSet<Guid> { root.Id };
        var frontier = new List<Guid> { root.Id };

        while (frontier.Count > 0)
        {
            var children = await _db.ArticleComments
                .AsNoTracking()
                .Where(c => c.ArticleId == articleId && c.ParentCommentId.HasValue && frontier.Contains(c.ParentCommentId.Value))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            frontier = children.Where(ids.Add).ToList();
        }

        await _db.ArticleComments
            .Where(c => c.ArticleId == articleId && ids.Contains(c.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<bool> TryCountUniqueViewAsync(Guid articleId, string? viewerIp, CancellationToken cancellationToken)
    {
        var normalizedIp = NormalizeIp(viewerIp);
        if (normalizedIp == null)
            return false;

        var now = DateTimeOffset.UtcNow;
        var affected = await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ArticleViews" ("ArticleId", "IpAddress", "ViewedAt")
            VALUES ({articleId}, {normalizedIp}, {now})
            ON CONFLICT ("ArticleId", "IpAddress") DO NOTHING
            """, cancellationToken);

        return affected > 0;
    }

    private static string? NormalizeIp(string? ip)
    {
        var candidate = ip?.Trim();
        if (string.IsNullOrWhiteSpace(candidate))
            return null;

        if (candidate.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase))
            candidate = candidate[7..];

        return candidate;
    }

}
