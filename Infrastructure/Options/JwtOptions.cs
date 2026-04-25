namespace SyriaSonsMovement.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SyriaSonsMovement";
    public string Audience { get; set; } = "SyriaSonsMovement";
    public string SigningKey { get; set; } = "";
    public int AccessTokenMinutes { get; set; } = 120;
}
