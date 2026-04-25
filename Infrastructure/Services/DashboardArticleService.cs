using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Persistence;
using SyriaSonsMovement.Infrastructure.Utilities;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardArticleService : IDashboardArticleService
{
    private readonly ApplicationDbContext _db;

    public DashboardArticleService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ArticleListItemDto>> ListAsync(ArticleDashboardQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = _db.Articles.AsNoTracking();
        if (query.CategoryId.HasValue)
            q = q.Where(a => a.CategoryId == query.CategoryId.Value);
        if (query.IsPublished.HasValue)
            q = q.Where(a => a.IsPublished == query.IsPublished.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(a => a.Title.Contains(s) || a.Slug.Contains(s));
        }

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleListItemDto
            {
                Id = a.Id,
                Title = a.Title,
                Slug = a.Slug,
                CoverImageUrl = a.CoverImageUrl,
                IsPublished = a.IsPublished,
                PublishedAt = a.PublishedAt,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                ViewCount = a.ViewCount,
                CreatedAt = a.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ArticleDetailDto?> GetByIdAsync(
        Guid id,
        int commentsPage = 1,
        int commentsPageSize = 20,
        int likesPage = 1,
        int likesPageSize = 20,
        CancellationToken cancellationToken = default)
    {
        commentsPage = Math.Max(1, commentsPage);
        commentsPageSize = Math.Clamp(commentsPageSize, 1, 100);
        likesPage = Math.Max(1, likesPage);
        likesPageSize = Math.Clamp(likesPageSize, 1, 100);

        var article = await _db.Articles
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (article == null)
            return null;

        var likeCount = await _db.ArticleLikes.CountAsync(l => l.ArticleId == article.Id, cancellationToken);
        var likesQuery = _db.ArticleLikes
            .AsNoTracking()
            .Include(l => l.User)
            .Where(l => l.ArticleId == article.Id);
        var totalLikes = await likesQuery.CountAsync(cancellationToken);
        var likedUsers = await likesQuery
            .OrderByDescending(l => l.CreatedAt)
            .Skip((likesPage - 1) * likesPageSize)
            .Take(likesPageSize)
            .Select(l => new ArticleLikeUserDto
            {
                UserId = l.UserId,
                UserDisplayName = l.User != null ? (l.User.DisplayName ?? l.User.Email) : null,
                UserEmail = l.User != null ? l.User.Email : null,
                LikedAt = l.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var topLevelCommentsBaseQuery = _db.ArticleComments
            .AsNoTracking()
            .Where(c =>
                c.ArticleId == article.Id
                && !c.IsDeleted
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
                        && (frontier.Contains(c.Id)
                            || (c.ParentCommentId.HasValue && frontier.Contains(c.ParentCommentId.Value))))
                    .ToListAsync(cancellationToken);

                var newNodes = currentLevel.Where(c => visitedIds.Add(c.Id)).ToList();
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
            ViewCount = article.ViewCount,
            LikeCount = likeCount,
            CategoryId = article.CategoryId,
            CategoryName = article.Category.Name,
            AuthorDisplayName = article.Author?.DisplayName ?? article.Author?.Email,
            LikedByCurrentUser = false,
            Comments = new PagedResult<ArticleCommentDto>
            {
                Items = tree,
                TotalCount = totalTopLevelComments,
                Page = commentsPage,
                PageSize = commentsPageSize,
            },
            LikedUsers = new PagedResult<ArticleLikeUserDto>
            {
                Items = likedUsers,
                TotalCount = totalLikes,
                Page = likesPage,
                PageSize = likesPageSize,
            },
        };
    }

    public async Task<ArticleDetailDto> CreateAsync(ArticleUpsertDto dto, Guid? authorId, CancellationToken cancellationToken = default)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new InvalidOperationException("Category not found.");

        var slug = SlugHelper.Normalize(dto.Slug);
        var now = DateTimeOffset.UtcNow;
        var publishedAt = dto.PublishedAt;
        if (dto.IsPublished && publishedAt == null)
            publishedAt = now;

        var entity = new Article
        {
            Id = Guid.NewGuid(),
            CategoryId = dto.CategoryId,
            AuthorId = authorId,
            Title = dto.Title,
            Slug = slug,
            Summary = dto.Summary,
            BodyHtml = dto.BodyHtml,
            CoverImageUrl = dto.CoverImageUrl,
            IsPublished = dto.IsPublished,
            PublishedAt = publishedAt,
            ViewCount = 0,
            CreatedAt = now,
        };

        _db.Articles.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(entity.Id, cancellationToken: cancellationToken))!;
    }

    public async Task<ArticleDetailDto?> UpdateAsync(Guid id, ArticleUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new InvalidOperationException("Category not found.");

        var entity = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (entity == null)
            return null;

        var now = DateTimeOffset.UtcNow;
        entity.CategoryId = dto.CategoryId;
        entity.Title = dto.Title;
        entity.Slug = SlugHelper.Normalize(dto.Slug);
        entity.Summary = dto.Summary;
        entity.BodyHtml = dto.BodyHtml;
        entity.CoverImageUrl = dto.CoverImageUrl;
        entity.IsPublished = dto.IsPublished;
        if (dto.IsPublished)
        {
            var publishedAt = dto.PublishedAt;
            if (publishedAt == null && entity.PublishedAt == null)
                publishedAt = now;
            entity.PublishedAt = publishedAt ?? entity.PublishedAt;
        }
        else
        {
            entity.PublishedAt = null;
        }
        entity.UpdatedAt = now;
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var n = await _db.Articles.Where(a => a.Id == id).ExecuteDeleteAsync(cancellationToken);
        return n > 0;
    }
}
