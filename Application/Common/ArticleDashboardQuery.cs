namespace SyriaSonsMovement.Application.Common;

public sealed class ArticleDashboardQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CategoryId { get; set; }
    public bool? IsPublished { get; set; }
    public string? Search { get; set; }
}
