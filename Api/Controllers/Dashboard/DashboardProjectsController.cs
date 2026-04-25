using Api;
using Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/projects")]
public class DashboardProjectsController : ControllerBase
{
    private readonly IDashboardProjectService _projects;

    public DashboardProjectsController(IDashboardProjectService projects)
    {
        _projects = projects;
    }

    [HttpGet("settings")]
    [Authorize(Policy = AuthorizationPolicies.ProjectsManage)]
    [ProducesResponseType(typeof(ProjectsPageSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProjectsPageSettingsDto>> GetSettings(CancellationToken cancellationToken)
    {
        return Ok(await _projects.GetSettingsAsync(cancellationToken));
    }

    [HttpPut("settings")]
    [Authorize(Policy = AuthorizationPolicies.ProjectsManage)]
    [ProducesResponseType(typeof(ProjectsPageSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProjectsPageSettingsDto>> UpsertSettings(
        [FromBody] ProjectsPageSettingsUpsertDto body,
        CancellationToken cancellationToken)
    {
        return Ok(await _projects.UpsertSettingsAsync(body, cancellationToken));
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ProjectsManage)]
    [ProducesResponseType(typeof(PagedResult<ProjectListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProjectListItemDto>>> List(
        [FromQuery] ProjectDashboardQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _projects.ListAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProjectsManage)]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var p = await _projects.GetByIdAsync(id, cancellationToken);
        if (p == null)
            return NotFound();
        return Ok(p);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ProjectsManage)]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectDetailDto>> Create([FromBody] ProjectUpsertDto body, CancellationToken cancellationToken)
    {
        try
        {
            var authorId = User.GetUserId();
            return Ok(await _projects.CreateAsync(body, authorId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProjectsManage)]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectDetailDto>> Update(Guid id, [FromBody] ProjectUpsertDto body, CancellationToken cancellationToken)
    {
        try
        {
            var p = await _projects.UpdateAsync(id, body, cancellationToken);
            if (p == null)
                return NotFound();
            return Ok(p);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProjectsManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _projects.DeleteAsync(id, cancellationToken);
        if (!ok)
            return NotFound();
        return NoContent();
    }
}
