using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/slider")]
public class DashboardSliderController : ControllerBase
{
    private readonly IDashboardSliderService _slider;

    public DashboardSliderController(IDashboardSliderService slider)
    {
        _slider = slider;
    }

    /// <summary>Search articles by title for linking a slide (requires at least 3 characters).</summary>
    [HttpGet("articles/search-by-title")]
    [Authorize(Policy = AuthorizationPolicies.SliderManage)]
    [ProducesResponseType(typeof(IReadOnlyList<ArticleTitleSearchItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ArticleTitleSearchItemDto>>> SearchArticlesByTitle(
        [FromQuery] string title,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 3)
            return BadRequest(new { message = "Title search requires at least 3 characters." });
        return Ok(await _slider.SearchArticlesByTitleAsync(title, take, cancellationToken));
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.SliderManage)]
    [ProducesResponseType(typeof(IReadOnlyList<SliderSlideDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SliderSlideDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _slider.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SliderManage)]
    [ProducesResponseType(typeof(SliderSlideDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SliderSlideDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var slide = await _slider.GetByIdAsync(id, cancellationToken);
        if (slide == null)
            return NotFound();
        return Ok(slide);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.SliderManage)]
    [ProducesResponseType(typeof(SliderSlideDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SliderSlideDto>> Create([FromBody] SliderSlideUpsertDto body, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _slider.CreateAsync(body, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SliderManage)]
    [ProducesResponseType(typeof(SliderSlideDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SliderSlideDto>> Update(Guid id, [FromBody] SliderSlideUpsertDto body, CancellationToken cancellationToken)
    {
        try
        {
            var slide = await _slider.UpdateAsync(id, body, cancellationToken);
            if (slide == null)
                return NotFound();
            return Ok(slide);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SliderManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _slider.DeleteAsync(id, cancellationToken);
        if (!ok)
            return NotFound();
        return NoContent();
    }
}
