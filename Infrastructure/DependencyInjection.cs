using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Security;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Identity;
using SyriaSonsMovement.Infrastructure.Options;
using SyriaSonsMovement.Infrastructure.Persistence;
using SyriaSonsMovement.Infrastructure.Services;

namespace SyriaSonsMovement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));
        services.Configure<UploadsOptions>(configuration.GetSection(UploadsOptions.SectionName));

        var cs = configuration.GetConnectionString("Default")
            ?? configuration["ConnectionStrings:Default"]
            ?? configuration["ConnectionStrings__Default"]
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? Environment.GetEnvironmentVariable("PG_CONNECTION_STRING")
            ?? TryBuildConnectionStringFromDatabaseUrl(
                configuration["DATABASE_URL"] ?? Environment.GetEnvironmentVariable("DATABASE_URL"))
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is not configured. Set ConnectionStrings:Default, ConnectionStrings__Default, PG_CONNECTION_STRING, or DATABASE_URL.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(cs));

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration is missing.");
        var keyBytes = Encoding.UTF8.GetBytes(jwt.SigningKey);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.SliderManage, p => p.RequireClaim("permission", Permissions.SliderManage));
            options.AddPolicy(AuthorizationPolicies.CategoriesManage, p => p.RequireClaim("permission", Permissions.CategoriesManage));
            options.AddPolicy(AuthorizationPolicies.ArticlesManage, p => p.RequireClaim("permission", Permissions.ArticlesManage));
            options.AddPolicy(AuthorizationPolicies.SocialManage, p => p.RequireClaim("permission", Permissions.SocialManage));
            options.AddPolicy(AuthorizationPolicies.CommentsModerate, p => p.RequireClaim("permission", Permissions.CommentsModerate));
            options.AddPolicy(AuthorizationPolicies.UsersManage, p => p.RequireClaim("permission", Permissions.UsersManage));
            options.AddPolicy(AuthorizationPolicies.RolesManage, p => p.RequireClaim("permission", Permissions.RolesManage));
            options.AddPolicy(AuthorizationPolicies.MediaUpload, p => p.RequireClaim("permission", Permissions.MediaUpload));
            options.AddPolicy(AuthorizationPolicies.AboutManage, p => p.RequireClaim("permission", Permissions.AboutManage));
            options.AddPolicy(AuthorizationPolicies.OrganizationManage, p => p.RequireClaim("permission", Permissions.OrganizationManage));
            options.AddPolicy(AuthorizationPolicies.ProjectsManage, p => p.RequireClaim("permission", Permissions.ProjectsManage));
            options.AddPolicy(AuthorizationPolicies.RolesOrUsersManage, p => p.RequireAssertion(ctx =>
                ctx.User.HasClaim("permission", Permissions.RolesManage)
                || ctx.User.HasClaim("permission", Permissions.UsersManage)));
        });

        services.AddScoped<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientArticleService, ClientArticleService>();
        services.AddScoped<IClientHomeService, ClientHomeService>();
        services.AddScoped<IDashboardSliderService, DashboardSliderService>();
        services.AddScoped<IDashboardCategoryService, DashboardCategoryService>();
        services.AddScoped<IDashboardArticleService, DashboardArticleService>();
        services.AddScoped<IDashboardSocialService, DashboardSocialService>();
        services.AddScoped<IDashboardAboutService, DashboardAboutService>();
        services.AddScoped<IClientOrganizationService, ClientOrganizationService>();
        services.AddScoped<IDashboardOrganizationService, DashboardOrganizationService>();
        services.AddScoped<IClientProjectService, ClientProjectService>();
        services.AddScoped<IDashboardProjectService, DashboardProjectService>();
        services.AddScoped<IDashboardCommentService, DashboardCommentService>();
        services.AddScoped<IMediaUploadService, MediaUploadService>();
        services.AddScoped<IDashboardPermissionService, DashboardPermissionService>();
        services.AddScoped<IDashboardRoleService, DashboardRoleService>();
        services.AddScoped<IDashboardUserService, DashboardUserService>();
        services.AddScoped<IDashboardMembershipService, DashboardMembershipService>();

        return services;
    }

    private static string? TryBuildConnectionStringFromDatabaseUrl(string? databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
            return null;

        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
            return null;

        if (!string.Equals(uri.Scheme, "postgres", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Scheme, "postgresql", StringComparison.OrdinalIgnoreCase))
            return null;

        var userInfoParts = uri.UserInfo.Split(':', 2);
        if (userInfoParts.Length < 2)
            return null;

        var username = Uri.UnescapeDataString(userInfoParts[0]);
        var password = Uri.UnescapeDataString(userInfoParts[1]);
        var database = uri.AbsolutePath.Trim('/');
        if (string.IsNullOrWhiteSpace(database))
            return null;

        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
