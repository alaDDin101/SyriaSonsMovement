namespace SyriaSonsMovement.Application.Dtos;

public sealed class PermissionDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public sealed class RoleListItemDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public int PermissionCount { get; init; }
}

public sealed class RoleDetailDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsSystemRole { get; init; }
    public required IReadOnlyList<PermissionDto> Permissions { get; init; }
}

public sealed class CreateRoleDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<string> PermissionNames { get; init; } = [];
}

public sealed class UpdateRoleDto
{
    /// <summary>Only applied to custom (non-system) roles.</summary>
    public string? Name { get; init; }
    public string? Description { get; init; }
}

public sealed class SetRolePermissionsDto
{
    public required IReadOnlyList<string> PermissionNames { get; init; }
}

public sealed class UserListItemDto
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
    public bool EmailConfirmed { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
}

public sealed class UserDetailDto
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
    public string? ProfileImageUrl { get; init; }
    public bool EmailConfirmed { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
}

public sealed class CreateUserDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? DisplayName { get; init; }
    public IReadOnlyList<string> RoleNames { get; init; } = [];
}

public sealed class UpdateUserDto
{
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public bool? EmailConfirmed { get; init; }
}

public sealed class AdminSetPasswordDto
{
    public required string NewPassword { get; init; }
}

public sealed class AssignUserRolesDto
{
    public required IReadOnlyList<string> RoleNames { get; init; }
}

public sealed class SetUserActiveDto
{
    public bool IsActive { get; init; }
}

public sealed class MembershipJoinRequestListItemDto
{
    public Guid UserId { get; init; }
    public required string FirstName { get; init; }
    public required string FatherName { get; init; }
    public required string LastName { get; init; }
    public required string BirthDate { get; init; }
    public required string Gender { get; init; }
    public required string City { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public string? Address { get; init; }
    public required string PreferredContactMethod { get; init; }

    public required string EducationLevel { get; init; }
    public required string Specialization { get; init; }
    public required string CurrentProfession { get; init; }
    public string? Employer { get; init; }

    public required string JoinReason { get; init; }
    public bool PreviouslyAffiliated { get; init; }
    public string? PreviousAffiliationDetails { get; init; }
    public required string ParticipationAreas { get; init; }
    public required string FocusIssues { get; init; }

    public required string Skills { get; init; }
    public string? PreviousExperiences { get; init; }
    public required string Languages { get; init; }

    public required string WeeklyVolunteerHours { get; init; }
    public bool FieldWorkReady { get; init; }
    public required string MobilityTravelAbility { get; init; }

    public bool CommitToPrinciples { get; init; }
    public bool InfoIsAccurate { get; init; }
    public bool AcceptPrivacyPolicy { get; init; }
    public DateTimeOffset RequestedAt { get; init; }
}
