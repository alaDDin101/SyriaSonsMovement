using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/roles")]
public class DashboardRolesController : ControllerBase
{
    private readonly IDashboardRoleService _roles;

    public DashboardRolesController(IDashboardRoleService roles)
    {
        _roles = roles;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RolesOrUsersManage)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleListItemDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _roles.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RolesManage)]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDetailDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roles.GetByIdAsync(id, cancellationToken);
        if (role == null)
            return NotFound();
        return Ok(role);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RolesManage)]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleDetailDto>> Create([FromBody] CreateRoleDto body, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _roles.CreateAsync(body, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RolesManage)]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleDetailDto>> Update(Guid id, [FromBody] UpdateRoleDto body, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roles.UpdateAsync(id, body, cancellationToken);
            if (role == null)
                return NotFound();
            return Ok(role);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = AuthorizationPolicies.RolesManage)]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleDetailDto>> SetPermissions(Guid id, [FromBody] SetRolePermissionsDto body, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roles.SetPermissionsAsync(id, body, cancellationToken);
            if (role == null)
                return NotFound();
            return Ok(role);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RolesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _roles.DeleteAsync(id, cancellationToken);
            if (!ok)
                return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
