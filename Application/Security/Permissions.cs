namespace SyriaSonsMovement.Application.Security;

public static class Permissions
{
    public const string SliderManage = "slider.manage";
    public const string CategoriesManage = "categories.manage";
    public const string ArticlesManage = "articles.manage";
    public const string SocialManage = "social.manage";
    public const string CommentsModerate = "comments.moderate";
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";
    public const string MediaUpload = "media.upload";
    public const string AboutManage = "about.manage";
    public const string OrganizationManage = "organization.manage";
    public const string ProjectsManage = "projects.manage";

    public static IReadOnlyList<string> All { get; } =
    [
        SliderManage,
        CategoriesManage,
        ArticlesManage,
        SocialManage,
        CommentsModerate,
        UsersManage,
        RolesManage,
        MediaUpload,
        AboutManage,
        OrganizationManage,
        ProjectsManage,
    ];
}
