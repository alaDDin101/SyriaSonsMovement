using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/organization")]
public class DashboardOrganizationController : ControllerBase
{
    private readonly IDashboardOrganizationService _organization;

    public DashboardOrganizationController(IDashboardOrganizationService organization)
    {
        _organization = organization;
    }

    [HttpGet("settings")]
    [Authorize(Policy = AuthorizationPolicies.OrganizationManage)]
    [ProducesResponseType(typeof(OrganizationalStructureSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganizationalStructureSettingsDto>> GetSettings(CancellationToken cancellationToken)
    {
        return Ok(await _organization.GetSettingsAsync(cancellationToken));
    }

    [HttpPut("settings")]
    [Authorize(Policy = AuthorizationPolicies.OrganizationManage)]
    [ProducesResponseType(typeof(OrganizationalStructureSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrganizationalStructureSettingsDto>> UpsertSettings(
        [FromBody] OrganizationalStructureSettingsUpsertDto body,
        CancellationToken cancellationToken)
    {
        return Ok(await _organization.UpsertSettingsAsync(body, cancellationToken));
    }

    [HttpGet("nodes")]
    [Authorize(Policy = AuthorizationPolicies.OrganizationManage)]
    [ProducesResponseType(typeof(IReadOnlyList<OrgStructureNodeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrgStructureNodeDto>>> ListNodes(CancellationToken cancellationToken)
    {
        return Ok(await _organization.ListNodesAsync(cancellationToken));
    }

    [HttpPost("nodes")]
    [Authorize(Policy = AuthorizationPolicies.OrganizationManage)]
    [ProducesResponseType(typeof(OrgStructureNodeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrgStructureNodeDto>> UpsertNode([FromBody] OrgStructureNodeUpsertDto body, CancellationToken cancellationToken)
    {
        return Ok(await _organization.UpsertNodeAsync(body, cancellationToken));
    }

    [HttpDelete("nodes/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.OrganizationManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNode(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _organization.DeleteNodeAsync(id, cancellationToken);
        if (!ok)
            return NotFound();
        return NoContent();
    }
}
