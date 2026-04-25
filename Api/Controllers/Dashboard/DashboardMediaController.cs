using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/media")]
public class DashboardMediaController : ControllerBase
{
    private readonly IMediaUploadService _media;

    public DashboardMediaController(IMediaUploadService media)
    {
        _media = media;
    }

    /// <summary>Upload an image file; returns a root-relative URL for use in CMS fields and on the public site.</summary>
    [HttpPost("upload")]
    [Authorize(Policy = AuthorizationPolicies.MediaUpload)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImageUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImageUploadResponseDto>> Upload([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _media.SaveImageAsync(stream, file.FileName, file.Length, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
