namespace SyriaSonsMovement.Application.Dtos;

public sealed class LoginRequestDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public sealed class JoinMovementRequestDto
{
    public required string FirstName { get; init; }
    public required string FatherName { get; init; }
    public required string LastName { get; init; }
    public required string BirthDate { get; init; }
    public required string Gender { get; init; }
    public required string City { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Password { get; init; }
    public string? Address { get; init; }
    public required string PreferredContactMethod { get; init; }

    public required string EducationLevel { get; init; }
    public required string Specialization { get; init; }
    public required string CurrentProfession { get; init; }
    public string? Employer { get; init; }

    public required string JoinReason { get; init; }
    public required bool PreviouslyAffiliated { get; init; }
    public string? PreviousAffiliationDetails { get; init; }
    public required string ParticipationAreas { get; init; }
    public required string FocusIssues { get; init; }

    public required string Skills { get; init; }
    public string? PreviousExperiences { get; init; }
    public required string Languages { get; init; }

    public required string WeeklyVolunteerHours { get; init; }
    public required bool FieldWorkReady { get; init; }
    public required string MobilityTravelAbility { get; init; }

    public required bool CommitToPrinciples { get; init; }
    public required bool InfoIsAccurate { get; init; }
    public required bool AcceptPrivacyPolicy { get; init; }
}

public sealed class TokenResponseDto
{
    public required string AccessToken { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required Guid UserId { get; init; }
    public string? Email { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
}

public sealed class MemberProfileDto
{
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
    public string JoinStatus { get; init; } = "unknown";
}

public sealed class UpdateMemberProfileDto
{
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
}
