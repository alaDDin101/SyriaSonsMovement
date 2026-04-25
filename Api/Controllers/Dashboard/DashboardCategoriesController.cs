using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;

namespace Api.Controllers.Dashboard;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Dashboard)]
[Route("api/dashboard/v1/categories")]
public class DashboardCategoriesController : ControllerBase
{
    private readonly IDashboardCategoryService _categories;

    public DashboardCategoriesController(IDashboardCategoryService categories)
    {
        _categories = categories;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CategoriesManage)]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryCardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryCardDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _categories.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CategoriesManage)]
    [ProducesResponseType(typeof(CategoryCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryCardDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var c = await _categories.GetByIdAsync(id, cancellationToken);
        if (c == null)
            return NotFound();
        return Ok(c);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CategoriesManage)]
    [ProducesResponseType(typeof(CategoryCardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryCardDto>> Create([FromBody] CategoryUpsertDto body, CancellationToken cancellationToken)
    {
        return Ok(await _categories.CreateAsync(body, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CategoriesManage)]
    [ProducesResponseType(typeof(CategoryCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryCardDto>> Update(Guid id, [FromBody] CategoryUpsertDto body, CancellationToken cancellationToken)
    {
        var c = await _categories.UpdateAsync(id, body, cancellationToken);
        if (c == null)
            return NotFound();
        return Ok(c);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CategoriesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _categories.DeleteAsync(id, cancellationToken);
        if (!ok)
            return NotFound();
        return NoContent();
    }
}
