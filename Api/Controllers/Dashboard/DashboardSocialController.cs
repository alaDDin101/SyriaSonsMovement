using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/social")]
public class DashboardSocialController : ControllerBase
{
    private readonly IDashboardSocialService _social;

    public DashboardSocialController(IDashboardSocialService social)
    {
        _social = social;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.SocialManage)]
    [ProducesResponseType(typeof(IReadOnlyList<SocialLinkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SocialLinkDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _social.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SocialManage)]
    [ProducesResponseType(typeof(SocialLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SocialLinkDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var link = await _social.GetByIdAsync(id, cancellationToken);
        if (link == null)
            return NotFound();
        return Ok(link);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.SocialManage)]
    [ProducesResponseType(typeof(SocialLinkDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SocialLinkDto>> Create([FromBody] SocialLinkUpsertDto body, CancellationToken cancellationToken)
    {
        return Ok(await _social.CreateAsync(body, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SocialManage)]
    [ProducesResponseType(typeof(SocialLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SocialLinkDto>> Update(Guid id, [FromBody] SocialLinkUpsertDto body, CancellationToken cancellationToken)
    {
        var link = await _social.UpdateAsync(id, body, cancellationToken);
        if (link == null)
            return NotFound();
        return Ok(link);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SocialManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _social.DeleteAsync(id, cancellationToken);
        if (!ok)
            return NotFound();
        return NoContent();
    }
}
