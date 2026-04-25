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
[Route("api/dashboard/v1/articles")]
public class DashboardArticlesController : ControllerBase
{
    private readonly IDashboardArticleService _articles;

    public DashboardArticlesController(IDashboardArticleService articles)
    {
        _articles = articles;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ArticlesManage)]
    [ProducesResponseType(typeof(PagedResult<ArticleListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ArticleListItemDto>>> List(
        [FromQuery] ArticleDashboardQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _articles.ListAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ArticlesManage)]
    [ProducesResponseType(typeof(ArticleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleDetailDto>> Get(
        Guid id,
        [FromQuery] int commentsPage = 1,
        [FromQuery] int commentsPageSize = 20,
        [FromQuery] int likesPage = 1,
        [FromQuery] int likesPageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var article = await _articles.GetByIdAsync(id, commentsPage, commentsPageSize, likesPage, likesPageSize, cancellationToken);
        if (article == null)
            return NotFound();
        return Ok(article);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ArticlesManage)]
    [ProducesResponseType(typeof(ArticleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ArticleDetailDto>> Create([FromBody] ArticleUpsertDto body, CancellationToken cancellationToken)
    {
        try
        {
            var authorId = User.GetUserId();
            return Ok(await _articles.CreateAsync(body, authorId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ArticlesManage)]
    [ProducesResponseType(typeof(ArticleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ArticleDetailDto>> Update(Guid id, [FromBody] ArticleUpsertDto body, CancellationToken cancellationToken)
    {
        try
        {
            var article = await _articles.UpdateAsync(id, body, cancellationToken);
            if (article == null)
                return NotFound();
            return Ok(article);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ArticlesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _articles.DeleteAsync(id, cancellationToken);
        if (!ok)
            return NotFound();
        return NoContent();
    }
}
