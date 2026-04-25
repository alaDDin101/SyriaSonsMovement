using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/comments")]
public class DashboardCommentsController : ControllerBase
{
    private readonly IDashboardCommentService _comments;

    public DashboardCommentsController(IDashboardCommentService comments)
    {
        _comments = comments;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CommentsModerate)]
    [ProducesResponseType(typeof(PagedResult<CommentModerationItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CommentModerationItemDto>>> List(
        [FromQuery] CommentDashboardQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _comments.ListAsync(query, cancellationToken));
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = AuthorizationPolicies.CommentsModerate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _comments.SetApprovedAsync(id, true, cancellationToken);
        if (!ok)
            return NotFound();
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = AuthorizationPolicies.CommentsModerate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _comments.SetApprovedAsync(id, false, cancellationToken);
        if (!ok)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CommentsModerate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _comments.SoftDeleteAsync(id, cancellationToken);
        if (!ok)
            return NotFound();
        return NoContent();
    }
}
