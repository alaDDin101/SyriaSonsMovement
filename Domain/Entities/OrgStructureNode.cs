using SyriaSonsMovement.Domain.Enums;

namespace SyriaSonsMovement.Domain.Entities;

/// <summary>
/// <see cref="OrgStructureNodeKind.Committee"/>: إدارة أو لجنة (جذر، بدون أب).
/// <see cref="OrgStructureNodeKind.Position"/>: منصب رسمي؛ إما تحت لجنة (<see cref="ParentId"/>) أو في المستوى الأعلى (مناصب عامة).
/// </summary>
public class OrgStructureNode
{
    public Guid Id { get; set; }

    public OrgStructureNodeKind Kind { get; set; }

    /// <summary>اسم اللجنة أو مسمى المنصب.</summary>
    public string Name { get; set; } = "";

    /// <summary>وصف اللجنة (HTML اختياري) أو ملاحظة للمنصب.</summary>
    public string? Description { get; set; }

    /// <summary>اسم شاغل المنصب (للنوع منصب فقط).</summary>
    public string? HolderName { get; set; }

    public Guid? ParentId { get; set; }
    public OrgStructureNode? Parent { get; set; }
    public ICollection<OrgStructureNode> Children { get; set; } = new List<OrgStructureNode>();

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTimeOffset UpdatedAt { get; set; }
}
