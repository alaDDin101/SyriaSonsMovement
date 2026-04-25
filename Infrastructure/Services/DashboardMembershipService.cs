using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardMembershipService : IDashboardMembershipService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardMembershipService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<PagedResult<MembershipJoinRequestListItemDto>> ListPendingAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var p = Math.Max(1, page);
        var size = Math.Clamp(pageSize, 1, 100);

        var pendingUserIds = _db.UserClaims
            .Where(c => c.ClaimType == MembershipClaimTypes.JoinStatus && c.ClaimValue == "pending")
            .Select(c => c.UserId);

        var users = _db.Users.AsNoTracking().Where(u => pendingUserIds.Contains(u.Id));

        var s = search?.Trim();
        if (!string.IsNullOrWhiteSpace(s))
        {
            users = users.Where(u =>
                u.Email!.Contains(s) ||
                (u.DisplayName != null && u.DisplayName.Contains(s)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(s)));
        }

        var total = await users.CountAsync(cancellationToken);
        var selectedUsers = await users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((p - 1) * size)
            .Take(size)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.DisplayName,
                u.PhoneNumber,
                u.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var userIds = selectedUsers.Select(u => u.Id).ToList();
        var claims = await _db.UserClaims
            .AsNoTracking()
            .Where(c => userIds.Contains(c.UserId))
            .Select(c => new { c.UserId, c.ClaimType, c.ClaimValue })
            .ToListAsync(cancellationToken);

        var claimMap = claims
            .GroupBy(c => c.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(x => x.ClaimType ?? string.Empty, x => x.ClaimValue ?? string.Empty));

        var items = selectedUsers.Select(u =>
        {
            claimMap.TryGetValue(u.Id, out var userClaims);
            return MapMembershipJoinRequest(u.Id, u.Email, u.PhoneNumber, u.CreatedAt, userClaims);
        }).ToList();

        return new PagedResult<MembershipJoinRequestListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = p,
            PageSize = size,
        };
    }

    public async Task<bool> ApproveAsync(Guid userId, Guid reviewedByUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        await SetJoinStatusAsync(user, "approved", reviewedByUserId, cancellationToken);
        user.EmailConfirmed = true;
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
            throw new InvalidOperationException(string.Join("; ", update.Errors.Select(e => e.Description)));

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(Roles.Member))
        {
            var add = await _userManager.AddToRoleAsync(user, Roles.Member);
            if (!add.Succeeded)
                throw new InvalidOperationException(string.Join("; ", add.Errors.Select(e => e.Description)));
        }

        return true;
    }

    public async Task<bool> RejectAsync(Guid userId, Guid reviewedByUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        await SetJoinStatusAsync(user, "rejected", reviewedByUserId, cancellationToken);
        return true;
    }

    public async Task<MembershipJoinRequestListItemDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Email, u.PhoneNumber, u.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return null;

        var claims = await _db.UserClaims
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new { c.ClaimType, c.ClaimValue })
            .ToListAsync(cancellationToken);

        if (claims.Count == 0)
            return null;

        var userClaims = claims.ToDictionary(x => x.ClaimType ?? string.Empty, x => x.ClaimValue ?? string.Empty);

        var hasJoinRequestData =
            userClaims.ContainsKey(MembershipClaimTypes.FirstName)
            || userClaims.ContainsKey(MembershipClaimTypes.JoinReason)
            || userClaims.ContainsKey(MembershipClaimTypes.RequestedAt);

        if (!hasJoinRequestData)
            return null;

        return MapMembershipJoinRequest(user.Id, user.Email, user.PhoneNumber, user.CreatedAt, userClaims);
    }

    private async Task SetJoinStatusAsync(
        ApplicationUser user,
        string status,
        Guid reviewedByUserId,
        CancellationToken cancellationToken)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var toReplace = claims.FirstOrDefault(c => c.Type == MembershipClaimTypes.JoinStatus);
        if (toReplace == null)
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(MembershipClaimTypes.JoinStatus, status));
        else
            await _userManager.ReplaceClaimAsync(user, toReplace, new System.Security.Claims.Claim(MembershipClaimTypes.JoinStatus, status));

        await UpsertClaimAsync(user, MembershipClaimTypes.ReviewedAt, DateTimeOffset.UtcNow.ToString("O"));
        await UpsertClaimAsync(user, MembershipClaimTypes.ReviewedBy, reviewedByUserId.ToString());
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

    private static MembershipJoinRequestListItemDto MapMembershipJoinRequest(
        Guid userId,
        string? email,
        string? phoneNumber,
        DateTimeOffset createdAt,
        IReadOnlyDictionary<string, string>? userClaims)
    {
        var claims = userClaims ?? new Dictionary<string, string>();
        var requestedAtRaw = claims.GetValueOrDefault(MembershipClaimTypes.RequestedAt);
        var requestedAt = DateTimeOffset.TryParse(requestedAtRaw, out var parsed) ? parsed : createdAt;

        return new MembershipJoinRequestListItemDto
        {
            UserId = userId,
            FirstName = claims.GetValueOrDefault(MembershipClaimTypes.FirstName) ?? "—",
            FatherName = claims.GetValueOrDefault(MembershipClaimTypes.FatherName) ?? "—",
            LastName = claims.GetValueOrDefault(MembershipClaimTypes.LastName) ?? "—",
            BirthDate = claims.GetValueOrDefault(MembershipClaimTypes.BirthDate) ?? "—",
            Gender = claims.GetValueOrDefault(MembershipClaimTypes.Gender) ?? "—",
            Email = email ?? "—",
            PhoneNumber = phoneNumber ?? "—",
            City = claims.GetValueOrDefault(MembershipClaimTypes.City) ?? "—",
            Address = claims.GetValueOrDefault(MembershipClaimTypes.Address),
            PreferredContactMethod = claims.GetValueOrDefault(MembershipClaimTypes.PreferredContactMethod) ?? "—",
            EducationLevel = claims.GetValueOrDefault(MembershipClaimTypes.EducationLevel) ?? "—",
            Specialization = claims.GetValueOrDefault(MembershipClaimTypes.Specialization) ?? "—",
            CurrentProfession = claims.GetValueOrDefault(MembershipClaimTypes.CurrentProfession) ?? "—",
            Employer = claims.GetValueOrDefault(MembershipClaimTypes.Employer),
            JoinReason = claims.GetValueOrDefault(MembershipClaimTypes.JoinReason) ?? "—",
            PreviouslyAffiliated = string.Equals(claims.GetValueOrDefault(MembershipClaimTypes.PreviouslyAffiliated), "true", StringComparison.OrdinalIgnoreCase),
            PreviousAffiliationDetails = claims.GetValueOrDefault(MembershipClaimTypes.PreviousAffiliationDetails),
            ParticipationAreas = claims.GetValueOrDefault(MembershipClaimTypes.ParticipationAreas) ?? "—",
            FocusIssues = claims.GetValueOrDefault(MembershipClaimTypes.FocusIssues) ?? "—",
            Skills = claims.GetValueOrDefault(MembershipClaimTypes.Skills) ?? "—",
            PreviousExperiences = claims.GetValueOrDefault(MembershipClaimTypes.PreviousExperiences),
            Languages = claims.GetValueOrDefault(MembershipClaimTypes.Languages) ?? "—",
            WeeklyVolunteerHours = claims.GetValueOrDefault(MembershipClaimTypes.WeeklyVolunteerHours) ?? "—",
            FieldWorkReady = string.Equals(claims.GetValueOrDefault(MembershipClaimTypes.FieldWorkReady), "true", StringComparison.OrdinalIgnoreCase),
            MobilityTravelAbility = claims.GetValueOrDefault(MembershipClaimTypes.MobilityTravelAbility) ?? "—",
            CommitToPrinciples = string.Equals(claims.GetValueOrDefault(MembershipClaimTypes.CommitToPrinciples), "true", StringComparison.OrdinalIgnoreCase),
            InfoIsAccurate = string.Equals(claims.GetValueOrDefault(MembershipClaimTypes.InfoIsAccurate), "true", StringComparison.OrdinalIgnoreCase),
            AcceptPrivacyPolicy = string.Equals(claims.GetValueOrDefault(MembershipClaimTypes.AcceptPrivacyPolicy), "true", StringComparison.OrdinalIgnoreCase),
            RequestedAt = requestedAt,
        };
    }
}
