using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Options;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Identity;

internal sealed class JwtTokenService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtOptions _jwt;

    public JwtTokenService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IOptions<JwtOptions> jwtOptions)
    {
        _db = db;
        _userManager = userManager;
        _jwt = jwtOptions.Value;
    }

    public async Task<TokenResponseDto> CreateTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var roleNames = roles as IReadOnlyList<string> ?? roles.ToList();
        var permissions = await GetPermissionNamesForRolesAsync(roleNames, cancellationToken);

        var claims = new List<System.Security.Claims.Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("userId", user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        foreach (var r in roles)
            claims.Add(new System.Security.Claims.Claim(ClaimTypes.Role, r));

        foreach (var p in permissions)
            claims.Add(new System.Security.Claims.Claim("permission", p));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTimeOffset.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.WriteToken(token);

        return new TokenResponseDto
        {
            AccessToken = jwt,
            ExpiresAt = expires,
            UserId = user.Id,
            Email = user.Email,
            Roles = roles.ToList(),
        };
    }

    private async Task<IReadOnlyList<string>> GetPermissionNamesForRolesAsync(
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken)
    {
        if (roleNames.Count == 0)
            return Array.Empty<string>();

        // Match Identity the same way role storage does: AspNetRoles.NormalizedName (uppercase), not fragile string IN on Name.
        var normalizedRoleKeys = roleNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (normalizedRoleKeys.Count == 0)
            return Array.Empty<string>();

        var roleIds = await _db.Roles
            .AsNoTracking()
            .Where(r => r.NormalizedName != null && normalizedRoleKeys.Contains(r.NormalizedName))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
            return Array.Empty<string>();

        return await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Join(_db.Permissions, rp => rp.PermissionId, p => p.Id, (_, p) => p.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
