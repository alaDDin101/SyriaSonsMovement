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
[Route("api/dashboard/v1/membership-requests")]
public class DashboardMembershipController : ControllerBase
{
    private readonly IDashboardMembershipService _membership;

    public DashboardMembershipController(IDashboardMembershipService membership)
    {
        _membership = membership;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(PagedResult<MembershipJoinRequestListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<MembershipJoinRequestListItemDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _membership.ListPendingAsync(page, pageSize, search, cancellationToken));
    }

    [HttpGet("{userId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(MembershipJoinRequestListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MembershipJoinRequestListItemDto>> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
    {
        var item = await _membership.GetByUserIdAsync(userId, cancellationToken);
        if (item == null)
            return NotFound();
        return Ok(item);
    }

    [HttpPost("{userId:guid}/approve")]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid userId, CancellationToken cancellationToken)
    {
        var reviewer = User.GetUserId();
        if (reviewer == null) return Unauthorized();
        var ok = await _membership.ApproveAsync(userId, reviewer.Value, cancellationToken);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpPost("{userId:guid}/reject")]
    [Authorize(Policy = AuthorizationPolicies.UsersManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid userId, CancellationToken cancellationToken)
    {
        var reviewer = User.GetUserId();
        if (reviewer == null) return Unauthorized();
        var ok = await _membership.RejectAsync(userId, reviewer.Value, cancellationToken);
        if (!ok) return NotFound();
        return NoContent();
    }
}
