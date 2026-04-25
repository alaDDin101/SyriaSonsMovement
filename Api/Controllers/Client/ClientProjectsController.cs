using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;

namespace Api.Controllers.Client;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Client)]
[Route("api/client/v1/projects")]
[AllowAnonymous]
public class ClientProjectsController : ControllerBase
{
    private readonly IClientProjectService _projects;

    public ClientProjectsController(IClientProjectService projects)
    {
        _projects = projects;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProjectsHubPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectsHubPageDto>> Hub(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] byte? section = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var hub = await _projects.GetHubAsync(page, pageSize, section, search, cancellationToken);
        if (hub == null)
            return NotFound();
        return Ok(hub);
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ProjectPublicDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectPublicDetailDto>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var p = await _projects.GetPublishedBySlugAsync(slug, cancellationToken);
        if (p == null)
            return NotFound();
        return Ok(p);
    }
}
