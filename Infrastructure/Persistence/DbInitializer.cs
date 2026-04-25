using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SyriaSonsMovement.Application.Security;
using SyriaSonsMovement.Domain;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Domain.Enums;

namespace SyriaSonsMovement.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task MigrateAndSeedAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        await SeedPermissionsAsync(db, cancellationToken);
        await SeedRolesAsync(db, roleManager, cancellationToken);
        await SeedAboutUsAsync(db, cancellationToken);
        await SeedOrganizationalStructureAsync(db, cancellationToken);
        await SeedProjectsPageAsync(db, cancellationToken);
        await SeedAdminUserAsync(userManager, configuration, logger, cancellationToken);
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        foreach (var name in Permissions.All)
        {
            var normalized = name.ToUpperInvariant();
            var exists = await db.Permissions.AnyAsync(p => p.NormalizedName == normalized, cancellationToken);
            if (exists)
                continue;
            db.Permissions.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Name = name,
                NormalizedName = normalized,
                Description = null,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRolesAsync(
        ApplicationDbContext db,
        RoleManager<ApplicationRole> roleManager,
        CancellationToken cancellationToken)
    {
        await EnsureRoleWithPermissionsAsync(db, roleManager, Roles.Member, [], cancellationToken);
        await EnsureRoleWithPermissionsAsync(db, roleManager, Roles.Admin, Permissions.All, cancellationToken);

        await EnsureRoleWithPermissionsAsync(db, roleManager, Roles.Editor,
        [
            Permissions.SliderManage,
            Permissions.CategoriesManage,
            Permissions.ArticlesManage,
            Permissions.SocialManage,
            Permissions.MediaUpload,
            Permissions.AboutManage,
            Permissions.OrganizationManage,
            Permissions.ProjectsManage,
        ], cancellationToken);

        await EnsureRoleWithPermissionsAsync(db, roleManager, Roles.Moderator,
        [
            Permissions.CommentsModerate,
        ], cancellationToken);
    }

    private static async Task EnsureRoleWithPermissionsAsync(
        ApplicationDbContext db,
        RoleManager<ApplicationRole> roleManager,
        string roleName,
        IReadOnlyList<string> permissionNames,
        CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            role = new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                CreatedAt = DateTimeOffset.UtcNow,
            };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        var permissionIds = await db.Permissions
            .Where(p => permissionNames.Contains(p.Name))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var existing = await db.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var pid in permissionIds.Where(pid => !existing.Contains(pid)))
        {
            db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = pid });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAboutUsAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        if (await db.AboutUsSettings.AnyAsync(x => x.Id == AboutUsIds.Singleton, cancellationToken))
            return;

        db.AboutUsSettings.Add(new AboutUsSettings
        {
            Id = AboutUsIds.Singleton,
            Title = "من نحن",
            LeadText = "حركة أبناء سوريا — توعية، عدالة، وبناء.",
            BodyHtml = "<p>عدّل هذا النص من لوحة التحكم. يمكنك إضافة صور وتنسيق النص.</p>",
            ImageUrl = null,
            IsVisible = true,
            SectionBackgroundColor = null,
            CardBackgroundColor = null,
            AccentColor = null,
            HeadingTextColor = null,
            BodyTextColor = null,
            MutedTextColor = null,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedOrganizationalStructureAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        if (!await db.OrganizationalStructureSettings.AnyAsync(x => x.Id == OrganizationalStructureIds.SettingsSingleton, cancellationToken))
        {
            db.OrganizationalStructureSettings.Add(new OrganizationalStructureSettings
            {
                Id = OrganizationalStructureIds.SettingsSingleton,
                Title = "الهيكل التنظيمي",
                LeadText = "عرض الإدارات واللجان والمناصب الرسمية.",
                IntroHtml = "<p>يمكنك تعديل هذا النص وإدارة اللجان والمناصب من لوحة التحكم.</p>",
                IsVisible = true,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        if (await db.OrgStructureNodes.AnyAsync(cancellationToken))
            return;

        var c1 = Guid.NewGuid();
        var c2 = Guid.NewGuid();
        var c3 = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        db.OrgStructureNodes.AddRange(
            new OrgStructureNode
            {
                Id = c1,
                Kind = OrgStructureNodeKind.Committee,
                Name = "اللجنة الإعلامية",
                Description = null,
                HolderName = null,
                ParentId = null,
                DisplayOrder = 0,
                IsActive = true,
                UpdatedAt = now,
            },
            new OrgStructureNode
            {
                Id = c2,
                Kind = OrgStructureNodeKind.Committee,
                Name = "اللجنة المالية",
                Description = null,
                HolderName = null,
                ParentId = null,
                DisplayOrder = 1,
                IsActive = true,
                UpdatedAt = now,
            },
            new OrgStructureNode
            {
                Id = c3,
                Kind = OrgStructureNodeKind.Committee,
                Name = "لجنة التنسيق",
                Description = null,
                HolderName = null,
                ParentId = null,
                DisplayOrder = 2,
                IsActive = true,
                UpdatedAt = now,
            },
            new OrgStructureNode
            {
                Id = Guid.NewGuid(),
                Kind = OrgStructureNodeKind.Position,
                Name = "منسّق إعلامي",
                HolderName = null,
                Description = null,
                ParentId = c1,
                DisplayOrder = 0,
                IsActive = true,
                UpdatedAt = now,
            },
            new OrgStructureNode
            {
                Id = Guid.NewGuid(),
                Kind = OrgStructureNodeKind.Position,
                Name = "رئيس الحركة",
                HolderName = null,
                Description = null,
                ParentId = null,
                DisplayOrder = 0,
                IsActive = true,
                UpdatedAt = now,
            });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedProjectsPageAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        if (!await db.ProjectsPageSettings.AnyAsync(x => x.Id == ProjectsPageIds.SettingsSingleton, cancellationToken))
        {
            db.ProjectsPageSettings.Add(new ProjectsPageSettings
            {
                Id = ProjectsPageIds.SettingsSingleton,
                Title = "المشاريع والمبادرات",
                LeadText = "ماذا تقدم الحركة عملياً — مشاريع خدمية وتوعوية — خطط مستقبلية.",
                IntroHtml = "<p>من هنا يمكنك إدارة صفحة المشاريع وإضافة مشاريع ومبادرات منفصلة عن المقالات.</p>",
                IsVisible = true,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var email = configuration["Seed:AdminEmail"] ?? "admin@example.com";
        var password = configuration["Seed:AdminPassword"] ?? "Admin@123456";
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
            return;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "Administrator",
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Seed admin user failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, Roles.Admin);
        logger.LogInformation("Seeded admin user {Email}", email);
    }
}
