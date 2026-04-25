using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;

namespace Api.Controllers.Client;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerDocumentNames.Client)]
[Route("api/client/v1/home")]
[AllowAnonymous]
public class ClientHomeController : ControllerBase
{
    private readonly IClientHomeService _home;
    private readonly IClientArticleService _articles;

    public ClientHomeController(IClientHomeService home, IClientArticleService articles)
    {
        _home = home;
        _articles = articles;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HomePageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HomePageDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _home.GetHomeAsync(cancellationToken));
    }

    [HttpGet("articles")]
    [ProducesResponseType(typeof(PagedResult<ArticleSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ArticleSummaryDto>>> GetArticles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _articles.ListPublishedAsync(page, pageSize, categoryId, search, cancellationToken));
    }

    [HttpGet("projects")]
    [ProducesResponseType(typeof(HomeProjectsSectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HomeProjectsSectionDto>> GetProjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6,
        CancellationToken cancellationToken = default)
    {
        var result = await _home.GetHomeProjectsPageAsync(page, pageSize, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }
}
