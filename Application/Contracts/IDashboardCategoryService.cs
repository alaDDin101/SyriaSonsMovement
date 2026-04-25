using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardCategoryService
{
    Task<IReadOnlyList<CategoryCardDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<CategoryCardDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryCardDto> CreateAsync(CategoryUpsertDto dto, CancellationToken cancellationToken = default);
    Task<CategoryCardDto?> UpdateAsync(Guid id, CategoryUpsertDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
