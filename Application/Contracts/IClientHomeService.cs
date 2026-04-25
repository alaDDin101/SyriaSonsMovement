using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IClientHomeService
{
    Task<HomePageDto> GetHomeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Paginated projects for the home strip. Returns <c>null</c> when the projects hub is hidden.
    /// </summary>
    Task<HomeProjectsSectionDto?> GetHomeProjectsPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
