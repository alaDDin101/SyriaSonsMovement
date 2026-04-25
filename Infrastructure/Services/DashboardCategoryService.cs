using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Persistence;
using SyriaSonsMovement.Infrastructure.Utilities;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardCategoryService : IDashboardCategoryService
{
    private readonly ApplicationDbContext _db;

    public DashboardCategoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CategoryCardDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryCardDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                BackgroundImageUrl = c.BackgroundImageUrl,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryCardDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryCardDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                BackgroundImageUrl = c.BackgroundImageUrl,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CategoryCardDto> CreateAsync(CategoryUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = SlugHelper.Normalize(dto.Slug),
            BackgroundImageUrl = dto.BackgroundImageUrl,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
        };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<CategoryCardDto?> UpdateAsync(Guid id, CategoryUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity == null)
            return null;

        entity.Name = dto.Name;
        entity.Slug = SlugHelper.Normalize(dto.Slug);
        entity.BackgroundImageUrl = dto.BackgroundImageUrl;
        entity.Description = dto.Description;
        entity.DisplayOrder = dto.DisplayOrder;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var n = await _db.Categories.Where(c => c.Id == id).ExecuteDeleteAsync(cancellationToken);
            return n > 0;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}
