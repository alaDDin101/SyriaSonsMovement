using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardSliderService
{
    /// <summary>Case-insensitive title substring search; intended for UI typeahead (caller enforces min length).</summary>
    Task<IReadOnlyList<ArticleTitleSearchItemDto>> SearchArticlesByTitleAsync(string title, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SliderSlideDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<SliderSlideDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SliderSlideDto> CreateAsync(SliderSlideUpsertDto dto, CancellationToken cancellationToken = default);
    Task<SliderSlideDto?> UpdateAsync(Guid id, SliderSlideUpsertDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
