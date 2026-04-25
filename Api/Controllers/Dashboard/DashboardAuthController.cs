using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/auth")]
public class DashboardAuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public DashboardAuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _auth.LoginDashboardAsync(request, cancellationToken);
            if (result == null)
                return Unauthorized();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
