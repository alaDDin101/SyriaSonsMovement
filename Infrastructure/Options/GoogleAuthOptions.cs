namespace SyriaSonsMovement.Infrastructure.Options;

public sealed class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    public string ClientId { get; set; } = "";
}
