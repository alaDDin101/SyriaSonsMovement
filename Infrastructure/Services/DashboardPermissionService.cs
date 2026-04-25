using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardPermissionService : IDashboardPermissionService
{
    private readonly ApplicationDbContext _db;

    public DashboardPermissionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PermissionDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
            })
            .ToListAsync(cancellationToken);
    }
}
