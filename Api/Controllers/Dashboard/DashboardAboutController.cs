using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/about")]
public class DashboardAboutController : ControllerBase
{
    private readonly IDashboardAboutService _about;

    public DashboardAboutController(IDashboardAboutService about)
    {
        _about = about;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AboutManage)]
    [ProducesResponseType(typeof(AboutUsSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AboutUsSettingsDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _about.GetAsync(cancellationToken));
    }

    [HttpPut]
    [Authorize(Policy = AuthorizationPolicies.AboutManage)]
    [ProducesResponseType(typeof(AboutUsSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AboutUsSettingsDto>> Upsert([FromBody] AboutUsUpsertDto body, CancellationToken cancellationToken)
    {
        return Ok(await _about.UpsertAsync(body, cancellationToken));
    }
}
