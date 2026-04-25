namespace SyriaSonsMovement.Application.Common;

public sealed class CommentDashboardQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ArticleId { get; set; }
    public bool? ApprovedOnly { get; set; }
}
