using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardSocialService
{
    Task<IReadOnlyList<SocialLinkDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<SocialLinkDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SocialLinkDto> CreateAsync(SocialLinkUpsertDto dto, CancellationToken cancellationToken = default);
    Task<SocialLinkDto?> UpdateAsync(Guid id, SocialLinkUpsertDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
