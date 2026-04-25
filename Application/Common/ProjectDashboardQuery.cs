namespace SyriaSonsMovement.Application.Common;

public sealed class ProjectDashboardQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public byte? Section { get; set; }
    public bool? IsPublished { get; set; }
    public string? Search { get; set; }
}
