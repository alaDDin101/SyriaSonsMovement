namespace SyriaSonsMovement.Application.Security;

/// <summary>ASP.NET Core authorization policy names (map to permission claims in Infrastructure).</summary>
public static class AuthorizationPolicies
{
    public const string SliderManage = "perm_slider_manage";
    public const string CategoriesManage = "perm_categories_manage";
    public const string ArticlesManage = "perm_articles_manage";
    public const string SocialManage = "perm_social_manage";
    public const string CommentsModerate = "perm_comments_moderate";
    public const string UsersManage = "perm_users_manage";
    public const string RolesManage = "perm_roles_manage";
    public const string MediaUpload = "perm_media_upload";
    public const string AboutManage = "perm_about_manage";
    public const string OrganizationManage = "perm_organization_manage";
    public const string ProjectsManage = "perm_projects_manage";

    /// <summary>Either <see cref="Permissions.RolesManage"/> or <see cref="Permissions.UsersManage"/> — shared read endpoints (e.g. permission catalog, role list for dropdowns).</summary>
    public const string RolesOrUsersManage = "perm_roles_or_users_manage";
}
