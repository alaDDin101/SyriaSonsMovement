namespace SyriaSonsMovement.Application.Common;

public sealed class UserManagementQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public Guid? RoleId { get; set; }
}
