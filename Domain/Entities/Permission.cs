namespace SyriaSonsMovement.Domain.Entities;

/// <summary>
/// Fine-grained permission (e.g. articles.manage, pages.publish). Assigned to roles via <see cref="RolePermission"/>.
/// </summary>
public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string NormalizedName { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
