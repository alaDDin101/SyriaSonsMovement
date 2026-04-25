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
[Route("api/dashboard/v1/users")]
public class DashboardUsersController : ControllerBase
{
    private readonly IDashboardUserService _users;

    public DashboardUsersController(IDashboardUserService users)
    {
        _users = users;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(PagedResult<UserListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserListItemDto>>> List([FromQuery] UserManagementQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _users.ListAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDetailDto>> Create([FromBody] CreateUserDto body, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _users.CreateAsync(body, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDetailDto>> Update(Guid id, [FromBody] UpdateUserDto body, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _users.UpdateAsync(id, body, cancellationToken);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/password")]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDetailDto>> SetPassword(Guid id, [FromBody] AdminSetPasswordDto body, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _users.SetPasswordAsync(id, body, cancellationToken);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/roles")]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDetailDto>> AssignRoles(Guid id, [FromBody] AssignUserRolesDto body, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _users.AssignRolesAsync(id, body, cancellationToken);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/activation")]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDetailDto>> SetActivation(Guid id, [FromBody] SetUserActiveDto body, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _users.SetActiveAsync(id, body.IsActive, cancellationToken);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId == null)
            return Unauthorized();

        try
        {
            var ok = await _users.DeleteAsync(id, currentUserId.Value, cancellationToken);
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
