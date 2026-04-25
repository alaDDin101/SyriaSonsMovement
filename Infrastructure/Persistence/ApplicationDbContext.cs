using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Domain.Entities;

namespace SyriaSonsMovement.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<HomeSliderSlide> HomeSliderSlides => Set<HomeSliderSlide>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleLike> ArticleLikes => Set<ArticleLike>();
    public DbSet<ArticleComment> ArticleComments => Set<ArticleComment>();
    public DbSet<ArticleView> ArticleViews => Set<ArticleView>();
    public DbSet<SocialMediaLink> SocialMediaLinks => Set<SocialMediaLink>();
    public DbSet<AboutUsSettings> AboutUsSettings => Set<AboutUsSettings>();
    public DbSet<OrganizationalStructureSettings> OrganizationalStructureSettings => Set<OrganizationalStructureSettings>();
    public DbSet<OrgStructureNode> OrgStructureNodes => Set<OrgStructureNode>();
    public DbSet<ProjectsPageSettings> ProjectsPageSettings => Set<ProjectsPageSettings>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
