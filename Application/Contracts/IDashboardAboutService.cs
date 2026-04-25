using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IDashboardAboutService
{
    Task<AboutUsSettingsDto> GetAsync(CancellationToken cancellationToken = default);
    Task<AboutUsSettingsDto> UpsertAsync(AboutUsUpsertDto dto, CancellationToken cancellationToken = default);
}
