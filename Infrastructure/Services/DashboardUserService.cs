using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Common;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardUserService : IDashboardUserService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public DashboardUserService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<PagedResult<UserListItemDto>> ListAsync(UserManagementQuery query, CancellationToken cancellationToken = default)
    {
        var q = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(u => u.Email!.Contains(s) || (u.DisplayName != null && u.DisplayName.Contains(s)));
        }

        if (query.RoleId.HasValue)
        {
            var userIds = _db.UserRoles.Where(ur => ur.RoleId == query.RoleId.Value).Select(ur => ur.UserId);
            q = q.Where(u => userIds.Contains(u.Id));
        }

        var total = await q.CountAsync(cancellationToken);
        var users = await q
            .OrderBy(u => u.Email)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = new List<UserListItemDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            items.Add(new UserListItemDto
            {
                Id = u.Id,
                Email = u.Email!,
                DisplayName = u.DisplayName,
                EmailConfirmed = u.EmailConfirmed,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                Roles = roles.OrderBy(r => r).ToList(),
            });
        }

        return new PagedResult<UserListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<UserDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return null;
        return await MapDetailAsync(user, cancellationToken);
    }

    public async Task<UserDetailDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedRoles = NormalizeAssignedRoles(dto.RoleNames);
        var user = new ApplicationUser
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            EmailConfirmed = true,
            DisplayName = dto.DisplayName,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        foreach (var roleName in normalizedRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                throw new InvalidOperationException($"Unknown role: '{roleName}'.");
            var add = await _userManager.AddToRoleAsync(user, roleName);
            if (!add.Succeeded)
                throw new InvalidOperationException(string.Join("; ", add.Errors.Select(e => e.Description)));
        }

        return (await MapDetailAsync(user, cancellationToken))!;
    }

    public async Task<UserDetailDto?> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(dto.Email.Trim(), user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var setEmail = await _userManager.SetEmailAsync(user, dto.Email.Trim());
            if (!setEmail.Succeeded)
                throw new InvalidOperationException(string.Join("; ", setEmail.Errors.Select(e => e.Description)));
            var setUserName = await _userManager.SetUserNameAsync(user, dto.Email.Trim());
            if (!setUserName.Succeeded)
                throw new InvalidOperationException(string.Join("; ", setUserName.Errors.Select(e => e.Description)));
        }

        if (dto.DisplayName != null)
            user.DisplayName = dto.DisplayName;

        if (dto.EmailConfirmed.HasValue)
            user.EmailConfirmed = dto.EmailConfirmed.Value;

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
            throw new InvalidOperationException(string.Join("; ", update.Errors.Select(e => e.Description)));

        return await MapDetailAsync(user, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        if (id == currentUserId)
            throw new InvalidOperationException("You cannot delete your own account.");

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return true;
    }

    public async Task<UserDetailDto?> SetPasswordAsync(Guid id, AdminSetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return null;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return await MapDetailAsync(user, cancellationToken);
    }

    public async Task<UserDetailDto?> AssignRolesAsync(Guid id, AssignUserRolesDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return null;

        var normalizedRoles = NormalizeAssignedRoles(dto.RoleNames);
        foreach (var roleName in normalizedRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                throw new InvalidOperationException($"Unknown role: '{roleName}'.");
        }

        var current = await _userManager.GetRolesAsync(user);
        var remove = await _userManager.RemoveFromRolesAsync(user, current);
        if (!remove.Succeeded)
            throw new InvalidOperationException(string.Join("; ", remove.Errors.Select(e => e.Description)));

        if (normalizedRoles.Count > 0)
        {
            var add = await _userManager.AddToRolesAsync(user, normalizedRoles.ToArray());
            if (!add.Succeeded)
                throw new InvalidOperationException(string.Join("; ", add.Errors.Select(e => e.Description)));
        }

        return await MapDetailAsync(user, cancellationToken);
    }

    public async Task<UserDetailDto?> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return null;

        user.LockoutEnd = isActive ? null : DateTimeOffset.UtcNow.AddYears(100);
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
            throw new InvalidOperationException(string.Join("; ", update.Errors.Select(e => e.Description)));

        return await MapDetailAsync(user, cancellationToken);
    }

    private async Task<UserDetailDto> MapDetailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = (await _userManager.GetRolesAsync(user)).OrderBy(r => r).ToList();
        return new UserDetailDto
        {
            Id = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            ProfileImageUrl = user.ProfileImageUrl,
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnd = user.LockoutEnd,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles,
        };
    }

    private static IReadOnlyList<string> NormalizeAssignedRoles(IReadOnlyList<string> roleNames)
    {
        var list = roleNames
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var needsMember = list.Any(r =>
            string.Equals(r, Roles.Editor, StringComparison.OrdinalIgnoreCase)
            || string.Equals(r, Roles.Moderator, StringComparison.OrdinalIgnoreCase));

        if (needsMember && !list.Contains(Roles.Member, StringComparer.OrdinalIgnoreCase))
            list.Add(Roles.Member);

        return list;
    }
}
