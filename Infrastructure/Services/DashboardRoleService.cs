using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Application.Security;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardRoleService : IDashboardRoleService
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardRoleService(
        ApplicationDbContext db,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<RoleListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync(cancellationToken);
        var counts = await _db.RolePermissions
            .GroupBy(rp => rp.RoleId)
            .Select(g => new { RoleId = g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Cnt, cancellationToken);

        return roles.Select(r => new RoleListItemDto
        {
            Id = r.Id,
            Name = r.Name!,
            Description = r.Description,
            CreatedAt = r.CreatedAt,
            PermissionCount = counts.TryGetValue(r.Id, out var c) ? c : 0,
        }).ToList();
    }

    public async Task<RoleDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            return null;
        return await MapDetailAsync(role, cancellationToken);
    }

    public async Task<RoleDetailDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Role name is required.");
        if (await _roleManager.RoleExistsAsync(name))
            throw new InvalidOperationException($"A role named '{name}' already exists.");

        var role = new ApplicationRole
        {
            Name = name,
            NormalizedName = _roleManager.NormalizeKey(name),
            Description = dto.Description,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await ReplaceRolePermissionsAsync(role.Id, dto.PermissionNames, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return (await MapDetailAsync(role, cancellationToken))!;
    }

    public async Task<RoleDetailDto?> UpdateAsync(Guid id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            return null;

        var system = IsSystemRoleName(role.Name);
        if (!string.IsNullOrWhiteSpace(dto.Name) && !string.Equals(dto.Name.Trim(), role.Name, StringComparison.Ordinal))
        {
            if (system)
                throw new InvalidOperationException("Cannot rename a system role.");
            var newName = dto.Name.Trim();
            if (await _roleManager.RoleExistsAsync(newName))
                throw new InvalidOperationException($"A role named '{newName}' already exists.");
            role.Name = newName;
            role.NormalizedName = _roleManager.NormalizeKey(newName);
        }

        if (dto.Description != null)
            role.Description = dto.Description;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return await MapDetailAsync(role, cancellationToken);
    }

    public async Task<RoleDetailDto?> SetPermissionsAsync(Guid id, SetRolePermissionsDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            return null;

        await ReplaceRolePermissionsAsync(role.Id, dto.PermissionNames, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(role, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            return false;

        if (IsSystemRoleName(role.Name))
            throw new InvalidOperationException("Cannot delete a system role.");

        var users = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (users.Count > 0)
            throw new InvalidOperationException("Cannot delete a role that is assigned to users.");

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return true;
    }

    private static bool IsSystemRoleName(string? name) =>
        name is Roles.Admin or Roles.Editor or Roles.Moderator;

    private async Task ReplaceRolePermissionsAsync(Guid roleId, IReadOnlyList<string> permissionNames, CancellationToken cancellationToken)
    {
        var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(cancellationToken);
        _db.RolePermissions.RemoveRange(existing);

        foreach (var name in permissionNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var perm = await _db.Permissions.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
            if (perm == null)
                throw new InvalidOperationException($"Unknown permission: '{name}'.");
            _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = perm.Id });
        }
    }

    private async Task<RoleDetailDto> MapDetailAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        var perms = await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == role.Id)
            .Join(_db.Permissions, rp => rp.PermissionId, p => p.Id, (_, p) => p)
            .OrderBy(p => p.Name)
            .Select(p => new PermissionDto { Id = p.Id, Name = p.Name, Description = p.Description })
            .ToListAsync(cancellationToken);

        return new RoleDetailDto
        {
            Id = role.Id,
            Name = role.Name!,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            IsSystemRole = IsSystemRoleName(role.Name),
            Permissions = perms,
        };
    }
}
