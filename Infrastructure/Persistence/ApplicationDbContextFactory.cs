using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SyriaSonsMovement.Infrastructure.Persistence;

/// <summary>
/// Used by EF Core CLI so <c>dotnet ef</c> uses the same connection as the Api host.
/// Optional override: set env <c>PG_CONNECTION_STRING</c> or <c>ConnectionStrings__Default</c>.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var cs = ResolveConnectionString();
        optionsBuilder.UseNpgsql(cs);
        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string ResolveConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("PG_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv;

        var apiDir = ResolveApiProjectDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDir)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var cs = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(cs))
            return cs;

        throw new InvalidOperationException(
            "Could not resolve database connection string. Set ConnectionStrings:Default in Api/appsettings.json, " +
            "or set environment variable PG_CONNECTION_STRING (or ConnectionStrings__Default).");
    }

    private static string ResolveApiProjectDirectory()
    {
        var cwd = Directory.GetCurrentDirectory();

        // Running from Api folder (e.g. dotnet ef with cwd = Api)
        if (File.Exists(Path.Combine(cwd, "appsettings.json")) &&
            File.Exists(Path.Combine(cwd, "Api.csproj")))
            return cwd;

        // Running from Infrastructure folder
        var siblingApi = Path.GetFullPath(Path.Combine(cwd, "..", "Api"));
        if (File.Exists(Path.Combine(siblingApi, "appsettings.json")))
            return siblingApi;

        // Walk up from cwd to find .../Api/appsettings.json (solution / repo root)
        for (var d = new DirectoryInfo(cwd); d != null; d = d.Parent)
        {
            var api = Path.Combine(d.FullName, "Api", "appsettings.json");
            if (File.Exists(api))
                return Path.Combine(d.FullName, "Api");
        }

        return cwd;
    }
}
