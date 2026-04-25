using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/permissions")]
public class DashboardPermissionsController : ControllerBase
{
    private readonly IDashboardPermissionService _permissions;

    public DashboardPermissionsController(IDashboardPermissionService permissions)
    {
        _permissions = permissions;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RolesOrUsersManage)]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PermissionDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _permissions.ListAsync(cancellationToken));
    }
}
