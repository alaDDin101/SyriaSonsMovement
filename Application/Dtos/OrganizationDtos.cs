namespace SyriaSonsMovement.Application.Dtos;

public sealed class OrgStructurePositionDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? HolderName { get; init; }
}

public sealed class OrgStructureCommitteeDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required IReadOnlyList<OrgStructurePositionDto> Positions { get; init; }
}

public sealed class OrgStructurePageDto
{
    public required string Title { get; init; }
    public string? LeadText { get; init; }
    public string? IntroHtml { get; init; }
    public required IReadOnlyList<OrgStructureCommitteeDto> Committees { get; init; }
    public required IReadOnlyList<OrgStructurePositionDto> RootPositions { get; init; }
}

public sealed class OrganizationalStructureSettingsDto
{
    public required string Title { get; init; }
    public string? LeadText { get; init; }
    public string? IntroHtml { get; init; }
    public bool IsVisible { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class OrganizationalStructureSettingsUpsertDto
{
    public required string Title { get; init; }
    public string? LeadText { get; init; }
    public string? IntroHtml { get; init; }
    public bool IsVisible { get; init; }
}

public sealed class OrgStructureNodeDto
{
    public required Guid Id { get; init; }
    public required byte Kind { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? HolderName { get; init; }
    public Guid? ParentId { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class OrgStructureNodeUpsertDto
{
    public Guid? Id { get; init; }
    public required byte Kind { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? HolderName { get; init; }
    public Guid? ParentId { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
}
