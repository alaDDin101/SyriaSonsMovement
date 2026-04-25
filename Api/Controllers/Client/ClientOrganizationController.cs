using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;

namespace Api.Controllers.Client;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Client)]
[Route("api/client/v1/organization")]
[AllowAnonymous]
public class ClientOrganizationController : ControllerBase
{
    private readonly IClientOrganizationService _organization;

    public ClientOrganizationController(IClientOrganizationService organization)
    {
        _organization = organization;
    }

    [HttpGet]
    [ProducesResponseType(typeof(OrgStructurePageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrgStructurePageDto>> Get(CancellationToken cancellationToken)
    {
        var page = await _organization.GetPublicAsync(cancellationToken);
        if (page == null)
            return NotFound();
        return Ok(page);
    }
}
