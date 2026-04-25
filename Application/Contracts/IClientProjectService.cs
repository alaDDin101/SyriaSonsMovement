using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IClientProjectService
{
    /// <param name="section">Optional project section (byte 1–3); omit for all sections.</param>
    Task<ProjectsHubPageDto?> GetHubAsync(
        int page,
        int pageSize,
        byte? section,
        string? search = null,
        CancellationToken cancellationToken = default);
    Task<ProjectPublicDetailDto?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
