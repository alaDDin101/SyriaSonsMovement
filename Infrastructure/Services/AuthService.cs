using Microsoft.AspNetCore.Identity;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Identity;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class AuthService : IAuthService
{
    private static readonly HashSet<string> DashboardRoles =
    [
        Roles.Admin,
        Roles.Editor,
        Roles.Moderator,
    ];

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _jwt;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        JwtTokenService jwt)
    {
        _userManager = userManager;
        _jwt = jwt;
    }

    public async Task<TokenResponseDto?> LoginClientAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return null;

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            throw new InvalidOperationException("تم تعطيل هذا الحساب من قبل الإدارة.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return null;

        var joinStatus = await GetClaimValueAsync(user, MembershipClaimTypes.JoinStatus);
        if (string.Equals(joinStatus, "pending", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("طلب الانضمام قيد المراجعة. يرجى انتظار موافقة الإدارة.");
        if (string.Equals(joinStatus, "rejected", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("تم رفض طلب الانضمام. يمكنك التواصل مع الإدارة للمراجعة.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any(HasClientAccessRole))
            throw new InvalidOperationException("لا يمكنك تسجيل الدخول قبل موافقة الإدارة على طلب الانضمام.");

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);
        return await _jwt.CreateTokenAsync(user, cancellationToken);
    }

    public async Task<TokenResponseDto?> LoginDashboardAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return null;

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any(r => DashboardRoles.Contains(r)))
            return null;
        if (roles.Any(r => string.Equals(r, Roles.Editor, StringComparison.OrdinalIgnoreCase) || string.Equals(r, Roles.Moderator, StringComparison.OrdinalIgnoreCase))
            && !roles.Any(r => string.Equals(r, Roles.Member, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("يجب أن يكون المستخدم منتسبًا (Member) قبل منحه صلاحيات الإدارة.");

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        return await _jwt.CreateTokenAsync(user, cancellationToken);
    }

    public async Task SubmitJoinRequestAsync(JoinMovementRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.PhoneNumber.Trim();
        var password = request.Password;
        var firstName = request.FirstName.Trim();
        var fatherName = request.FatherName.Trim();
        var lastName = request.LastName.Trim();
        var birthDate = request.BirthDate.Trim();
        var gender = request.Gender.Trim();
        var city = request.City.Trim();
        var address = request.Address?.Trim();
        var preferredContactMethod = request.PreferredContactMethod.Trim();

        var educationLevel = request.EducationLevel.Trim();
        var specialization = request.Specialization.Trim();
        var currentProfession = request.CurrentProfession.Trim();
        var employer = request.Employer?.Trim();

        var joinReason = request.JoinReason.Trim();
        var previousAffiliationDetails = request.PreviousAffiliationDetails?.Trim();
        var participationAreas = request.ParticipationAreas.Trim();
        var focusIssues = request.FocusIssues.Trim();

        var skills = request.Skills.Trim();
        var previousExperiences = request.PreviousExperiences?.Trim();
        var languages = request.Languages.Trim();

        var weeklyVolunteerHours = request.WeeklyVolunteerHours.Trim();
        var mobilityTravelAbility = request.MobilityTravelAbility.Trim();

        if (!request.CommitToPrinciples || !request.InfoIsAccurate || !request.AcceptPrivacyPolicy)
            throw new InvalidOperationException("يجب الموافقة على التعهدات الأساسية قبل إرسال الطلب.");

        if (firstName.Length < 2 || fatherName.Length < 2 || lastName.Length < 2 || city.Length < 2)
            throw new InvalidOperationException("يرجى تعبئة الاسم الثلاثي ومكان الإقامة بشكل صحيح.");
        if (!IsArabicLike(firstName) || !IsArabicLike(fatherName) || !IsArabicLike(lastName) || !IsArabicLike(city))
            throw new InvalidOperationException("يرجى كتابة الاسم الثلاثي ومكان الإقامة باللغة العربية.");

        if (birthDate.Length < 8 || gender.Length < 2)
            throw new InvalidOperationException("يرجى إدخال تاريخ الميلاد والجنس بشكل صحيح.");
        if (phone.Length < 8)
            throw new InvalidOperationException("يرجى إدخال رقم هاتف صحيح.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new InvalidOperationException("يرجى إدخال كلمة مرور من 8 أحرف على الأقل.");
        if (educationLevel.Length < 2 || specialization.Length < 2 || currentProfession.Length < 2)
            throw new InvalidOperationException("يرجى تعبئة البيانات التعليمية والمهنية.");
        if (joinReason.Length < 20 || participationAreas.Length < 2 || focusIssues.Length < 2)
            throw new InvalidOperationException("يرجى توضيح سبب الانضمام ومجالات المشاركة والقضايا المهتم بها.");
        if (skills.Length < 2 || languages.Length < 2 || weeklyVolunteerHours.Length < 1 || mobilityTravelAbility.Length < 2)
            throw new InvalidOperationException("يرجى تعبئة بيانات المهارات واللغات والالتزام.");
        if (request.PreviouslyAffiliated && string.IsNullOrWhiteSpace(previousAffiliationDetails))
            throw new InvalidOperationException("يرجى توضيح تفاصيل الانتماء السياسي السابق.");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false,
                DisplayName = $"{firstName} {fatherName} {lastName}",
                PhoneNumber = phone,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            var created = await _userManager.CreateAsync(user, password);
            if (!created.Succeeded)
                throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));
        }
        else
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, resetToken, password);
            if (!reset.Succeeded)
                throw new InvalidOperationException(string.Join("; ", reset.Errors.Select(e => e.Description)));
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Any(HasClientAccessRole))
            throw new InvalidOperationException("هذا البريد يملك حساباً مفعلًا بالفعل.");

        await UpsertClaimAsync(user, MembershipClaimTypes.JoinStatus, "pending");
        await UpsertClaimAsync(user, MembershipClaimTypes.FirstName, firstName);
        await UpsertClaimAsync(user, MembershipClaimTypes.FatherName, fatherName);
        await UpsertClaimAsync(user, MembershipClaimTypes.LastName, lastName);
        await UpsertClaimAsync(user, MembershipClaimTypes.BirthDate, birthDate);
        await UpsertClaimAsync(user, MembershipClaimTypes.Gender, gender);
        await UpsertClaimAsync(user, MembershipClaimTypes.City, city);
        await UpsertClaimAsync(user, MembershipClaimTypes.Email, email);
        await UpsertClaimAsync(user, MembershipClaimTypes.PhoneNumber, phone);
        await UpsertClaimAsync(user, MembershipClaimTypes.Address, address ?? "");
        await UpsertClaimAsync(user, MembershipClaimTypes.PreferredContactMethod, preferredContactMethod);
        await UpsertClaimAsync(user, MembershipClaimTypes.EducationLevel, educationLevel);
        await UpsertClaimAsync(user, MembershipClaimTypes.Specialization, specialization);
        await UpsertClaimAsync(user, MembershipClaimTypes.CurrentProfession, currentProfession);
        await UpsertClaimAsync(user, MembershipClaimTypes.Employer, employer ?? "");
        await UpsertClaimAsync(user, MembershipClaimTypes.JoinReason, joinReason);
        await UpsertClaimAsync(user, MembershipClaimTypes.PreviouslyAffiliated, BoolToString(request.PreviouslyAffiliated));
        await UpsertClaimAsync(user, MembershipClaimTypes.PreviousAffiliationDetails, previousAffiliationDetails ?? "");
        await UpsertClaimAsync(user, MembershipClaimTypes.ParticipationAreas, participationAreas);
        await UpsertClaimAsync(user, MembershipClaimTypes.FocusIssues, focusIssues);
        await UpsertClaimAsync(user, MembershipClaimTypes.Skills, skills);
        await UpsertClaimAsync(user, MembershipClaimTypes.PreviousExperiences, previousExperiences ?? "");
        await UpsertClaimAsync(user, MembershipClaimTypes.Languages, languages);
        await UpsertClaimAsync(user, MembershipClaimTypes.WeeklyVolunteerHours, weeklyVolunteerHours);
        await UpsertClaimAsync(user, MembershipClaimTypes.FieldWorkReady, BoolToString(request.FieldWorkReady));
        await UpsertClaimAsync(user, MembershipClaimTypes.MobilityTravelAbility, mobilityTravelAbility);
        await UpsertClaimAsync(user, MembershipClaimTypes.CommitToPrinciples, BoolToString(request.CommitToPrinciples));
        await UpsertClaimAsync(user, MembershipClaimTypes.InfoIsAccurate, BoolToString(request.InfoIsAccurate));
        await UpsertClaimAsync(user, MembershipClaimTypes.AcceptPrivacyPolicy, BoolToString(request.AcceptPrivacyPolicy));
        await UpsertClaimAsync(user, MembershipClaimTypes.RequestedAt, DateTimeOffset.UtcNow.ToString("O"));
    }

    public async Task<MemberProfileDto?> GetClientProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return null;

        await EnsureCanManageProfileAsync(user);
        return await MapMemberProfileAsync(user);
    }

    public async Task<MemberProfileDto?> UpdateClientProfileAsync(
        Guid userId,
        UpdateMemberProfileDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return null;

        await EnsureCanManageProfileAsync(user);

        var firstName = request.FirstName.Trim();
        var fatherName = request.FatherName.Trim();
        var lastName = request.LastName.Trim();
        var birthDate = request.BirthDate.Trim();
        var gender = request.Gender.Trim();
        var city = request.City.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.PhoneNumber.Trim();
        var address = request.Address?.Trim();
        var preferredContactMethod = request.PreferredContactMethod.Trim();
        var educationLevel = request.EducationLevel.Trim();
        var specialization = request.Specialization.Trim();
        var currentProfession = request.CurrentProfession.Trim();
        var employer = request.Employer?.Trim();
        var joinReason = request.JoinReason.Trim();
        var previousAffiliationDetails = request.PreviousAffiliationDetails?.Trim();
        var participationAreas = request.ParticipationAreas.Trim();
        var focusIssues = request.FocusIssues.Trim();
        var skills = request.Skills.Trim();
        var previousExperiences = request.PreviousExperiences?.Trim();
        var languages = request.Languages.Trim();
        var weeklyVolunteerHours = request.WeeklyVolunteerHours.Trim();
        var mobilityTravelAbility = request.MobilityTravelAbility.Trim();

        if (firstName.Length < 2 || fatherName.Length < 2 || lastName.Length < 2 || city.Length < 2)
            throw new InvalidOperationException("يرجى تعبئة الاسم الثلاثي ومكان الإقامة بشكل صحيح.");
        if (!IsArabicLike(firstName) || !IsArabicLike(fatherName) || !IsArabicLike(lastName) || !IsArabicLike(city))
            throw new InvalidOperationException("يرجى كتابة الاسم الثلاثي ومكان الإقامة باللغة العربية.");
        if (birthDate.Length < 8 || gender.Length < 2)
            throw new InvalidOperationException("يرجى إدخال تاريخ الميلاد والجنس بشكل صحيح.");
        if (phone.Length < 8)
            throw new InvalidOperationException("يرجى إدخال رقم هاتف صحيح.");
        if (educationLevel.Length < 2 || specialization.Length < 2 || currentProfession.Length < 2)
            throw new InvalidOperationException("يرجى تعبئة البيانات التعليمية والمهنية.");
        if (joinReason.Length < 20 || participationAreas.Length < 2 || focusIssues.Length < 2)
            throw new InvalidOperationException("يرجى توضيح سبب الانضمام ومجالات المشاركة والقضايا المهتم بها.");
        if (skills.Length < 2 || languages.Length < 2 || weeklyVolunteerHours.Length < 1 || mobilityTravelAbility.Length < 2)
            throw new InvalidOperationException("يرجى تعبئة بيانات المهارات واللغات والالتزام.");
        if (request.PreviouslyAffiliated && string.IsNullOrWhiteSpace(previousAffiliationDetails))
            throw new InvalidOperationException("يرجى توضيح تفاصيل الانتماء السياسي السابق.");
        if (!request.CommitToPrinciples || !request.InfoIsAccurate || !request.AcceptPrivacyPolicy)
            throw new InvalidOperationException("يجب الإقرار بالتعهدات الأساسية.");

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var setEmail = await _userManager.SetEmailAsync(user, email);
            if (!setEmail.Succeeded)
                throw new InvalidOperationException(string.Join("; ", setEmail.Errors.Select(e => e.Description)));

            var setUserName = await _userManager.SetUserNameAsync(user, email);
            if (!setUserName.Succeeded)
                throw new InvalidOperationException(string.Join("; ", setUserName.Errors.Select(e => e.Description)));
        }

        user.PhoneNumber = phone;
        user.DisplayName = $"{firstName} {fatherName} {lastName}";
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
            throw new InvalidOperationException(string.Join("; ", update.Errors.Select(e => e.Description)));

        await UpsertClaimAsync(user, MembershipClaimTypes.FirstName, firstName);
        await UpsertClaimAsync(user, MembershipClaimTypes.FatherName, fatherName);
        await UpsertClaimAsync(user, MembershipClaimTypes.LastName, lastName);
        await UpsertClaimAsync(user, MembershipClaimTypes.BirthDate, birthDate);
        await UpsertClaimAsync(user, MembershipClaimTypes.Gender, gender);
        await UpsertClaimAsync(user, MembershipClaimTypes.City, city);
        await UpsertClaimAsync(user, MembershipClaimTypes.Email, email);
        await UpsertClaimAsync(user, MembershipClaimTypes.PhoneNumber, phone);
        await UpsertClaimAsync(user, MembershipClaimTypes.Address, address ?? "");
        await UpsertClaimAsync(user, MembershipClaimTypes.PreferredContactMethod, preferredContactMethod);
        await UpsertClaimAsync(user, MembershipClaimTypes.EducationLevel, educationLevel);
        await UpsertClaimAsync(user, MembershipClaimTypes.Specialization, specialization);
        await UpsertClaimAsync(user, MembershipClaimTypes.CurrentProfession, currentProfession);
        await UpsertClaimAsync(user, MembershipClaimTypes.Employer, employer ?? "");
        await UpsertClaimAsync(user, MembershipClaimTypes.JoinReason, joinReason);
        await UpsertClaimAsync(user, MembershipClaimTypes.PreviouslyAffiliated, BoolToString(request.PreviouslyAffiliated));
        await UpsertClaimAsync(user, MembershipClaimTypes.PreviousAffiliationDetails, previousAffiliationDetails ?? "");
        await UpsertClaimAsync(user, MembershipClaimTypes.ParticipationAreas, participationAreas);
        await UpsertClaimAsync(user, MembershipClaimTypes.FocusIssues, focusIssues);
        await UpsertClaimAsync(user, MembershipClaimTypes.Skills, skills);
        await UpsertClaimAsync(user, MembershipClaimTypes.PreviousExperiences, previousExperiences ?? "");
        await UpsertClaimAsync(user, MembershipClaimTypes.Languages, languages);
        await UpsertClaimAsync(user, MembershipClaimTypes.WeeklyVolunteerHours, weeklyVolunteerHours);
        await UpsertClaimAsync(user, MembershipClaimTypes.FieldWorkReady, BoolToString(request.FieldWorkReady));
        await UpsertClaimAsync(user, MembershipClaimTypes.MobilityTravelAbility, mobilityTravelAbility);
        await UpsertClaimAsync(user, MembershipClaimTypes.CommitToPrinciples, BoolToString(request.CommitToPrinciples));
        await UpsertClaimAsync(user, MembershipClaimTypes.InfoIsAccurate, BoolToString(request.InfoIsAccurate));
        await UpsertClaimAsync(user, MembershipClaimTypes.AcceptPrivacyPolicy, BoolToString(request.AcceptPrivacyPolicy));

        return await MapMemberProfileAsync(user);
    }

    private static bool HasClientAccessRole(string roleName) =>
        string.Equals(roleName, Roles.Member, StringComparison.OrdinalIgnoreCase)
        || string.Equals(roleName, Roles.Admin, StringComparison.OrdinalIgnoreCase)
        || string.Equals(roleName, Roles.Editor, StringComparison.OrdinalIgnoreCase)
        || string.Equals(roleName, Roles.Moderator, StringComparison.OrdinalIgnoreCase);

    private static bool IsArabicLike(string value) => value.Any(ch => ch is >= '\u0600' and <= '\u06FF');
    private static string BoolToString(bool value) => value ? "true" : "false";

    private async Task<string?> GetClaimValueAsync(ApplicationUser user, string claimType)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        return claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    private async Task UpsertClaimAsync(ApplicationUser user, string claimType, string claimValue)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var existing = claims.FirstOrDefault(c => c.Type == claimType);
        if (existing == null)
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(claimType, claimValue));
        else
            await _userManager.ReplaceClaimAsync(user, existing, new System.Security.Claims.Claim(claimType, claimValue));
    }

    private async Task EnsureCanManageProfileAsync(ApplicationUser user)
    {
        var joinStatus = await GetClaimValueAsync(user, MembershipClaimTypes.JoinStatus);
        if (!string.Equals(joinStatus, "approved", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("يمكن تعديل الملف الشخصي بعد الموافقة على طلب الانضمام فقط.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any(HasClientAccessRole))
            throw new InvalidOperationException("لا تملك صلاحية الوصول إلى الملف الشخصي.");
    }

    private async Task<MemberProfileDto> MapMemberProfileAsync(ApplicationUser user)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var map = claims.ToDictionary(c => c.Type, c => c.Value);

        return new MemberProfileDto
        {
            FirstName = map.GetValueOrDefault(MembershipClaimTypes.FirstName) ?? "",
            FatherName = map.GetValueOrDefault(MembershipClaimTypes.FatherName) ?? "",
            LastName = map.GetValueOrDefault(MembershipClaimTypes.LastName) ?? "",
            BirthDate = map.GetValueOrDefault(MembershipClaimTypes.BirthDate) ?? "",
            Gender = map.GetValueOrDefault(MembershipClaimTypes.Gender) ?? "",
            City = map.GetValueOrDefault(MembershipClaimTypes.City) ?? "",
            Email = user.Email ?? map.GetValueOrDefault(MembershipClaimTypes.Email) ?? "",
            PhoneNumber = user.PhoneNumber ?? map.GetValueOrDefault(MembershipClaimTypes.PhoneNumber) ?? "",
            Address = map.GetValueOrDefault(MembershipClaimTypes.Address),
            PreferredContactMethod = map.GetValueOrDefault(MembershipClaimTypes.PreferredContactMethod) ?? "",
            EducationLevel = map.GetValueOrDefault(MembershipClaimTypes.EducationLevel) ?? "",
            Specialization = map.GetValueOrDefault(MembershipClaimTypes.Specialization) ?? "",
            CurrentProfession = map.GetValueOrDefault(MembershipClaimTypes.CurrentProfession) ?? "",
            Employer = map.GetValueOrDefault(MembershipClaimTypes.Employer),
            JoinReason = map.GetValueOrDefault(MembershipClaimTypes.JoinReason) ?? "",
            PreviouslyAffiliated = string.Equals(map.GetValueOrDefault(MembershipClaimTypes.PreviouslyAffiliated), "true", StringComparison.OrdinalIgnoreCase),
            PreviousAffiliationDetails = map.GetValueOrDefault(MembershipClaimTypes.PreviousAffiliationDetails),
            ParticipationAreas = map.GetValueOrDefault(MembershipClaimTypes.ParticipationAreas) ?? "",
            FocusIssues = map.GetValueOrDefault(MembershipClaimTypes.FocusIssues) ?? "",
            Skills = map.GetValueOrDefault(MembershipClaimTypes.Skills) ?? "",
            PreviousExperiences = map.GetValueOrDefault(MembershipClaimTypes.PreviousExperiences),
            Languages = map.GetValueOrDefault(MembershipClaimTypes.Languages) ?? "",
            WeeklyVolunteerHours = map.GetValueOrDefault(MembershipClaimTypes.WeeklyVolunteerHours) ?? "",
            FieldWorkReady = string.Equals(map.GetValueOrDefault(MembershipClaimTypes.FieldWorkReady), "true", StringComparison.OrdinalIgnoreCase),
            MobilityTravelAbility = map.GetValueOrDefault(MembershipClaimTypes.MobilityTravelAbility) ?? "",
            CommitToPrinciples = string.Equals(map.GetValueOrDefault(MembershipClaimTypes.CommitToPrinciples), "true", StringComparison.OrdinalIgnoreCase),
            InfoIsAccurate = string.Equals(map.GetValueOrDefault(MembershipClaimTypes.InfoIsAccurate), "true", StringComparison.OrdinalIgnoreCase),
            AcceptPrivacyPolicy = string.Equals(map.GetValueOrDefault(MembershipClaimTypes.AcceptPrivacyPolicy), "true", StringComparison.OrdinalIgnoreCase),
            JoinStatus = map.GetValueOrDefault(MembershipClaimTypes.JoinStatus) ?? "unknown",
        };
    }
}
