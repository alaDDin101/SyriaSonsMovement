using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Api.HealthChecks;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database");
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(SwaggerDocumentNames.Dashboard, new OpenApiInfo
            {
                Title = "Syria Sons Movement — Dashboard API",
                Version = "v1",
                Description = "CMS and moderation. Authenticate with POST /api/dashboard/v1/auth/login.",
            });
            c.SwaggerDoc(SwaggerDocumentNames.Client, new OpenApiInfo
            {
                Title = "Syria Sons Movement — Client API",
                Version = "v1",
                Description = "Public site and Google sign-in. Use POST /api/client/v1/auth/google for JWT.",
            });

            c.DocInclusionPredicate((docName, apiDesc) =>
                string.Equals(apiDesc.GroupName, docName, StringComparison.OrdinalIgnoreCase));

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT from dashboard login or Google client login.",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                }] = Array.Empty<string>(),
            });
        });

        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
            p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint($"/swagger/{SwaggerDocumentNames.Dashboard}/swagger.json", "Dashboard API");
                o.SwaggerEndpoint($"/swagger/{SwaggerDocumentNames.Client}/swagger.json", "Client API");
            });
        }
        app.UseSwagger();
        app.UseSwaggerUI(o =>
        {
            o.SwaggerEndpoint($"/swagger/{SwaggerDocumentNames.Dashboard}/swagger.json", "Dashboard API");
            o.SwaggerEndpoint($"/swagger/{SwaggerDocumentNames.Client}/swagger.json", "Client API");
        });
        using (var scope = app.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
            await DbInitializer.MigrateAndSeedAsync(ctx, userManager, roleManager, app.Configuration, logger);
        }

        // In local development, website/dev-server proxy often targets HTTP backend.
        // Redirecting to HTTPS can strip Authorization headers on redirected requests,
        // which causes authenticated client actions (like/comment) to return 401.
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();
        app.UseDeveloperExceptionPage();
        app.UseCors();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHealthChecks("/health");
        app.MapControllers();

        await app.RunAsync();
    }
}
