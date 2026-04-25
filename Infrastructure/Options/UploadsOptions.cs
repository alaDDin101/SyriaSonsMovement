namespace SyriaSonsMovement.Infrastructure.Options;

public sealed class UploadsOptions
{
    public const string SectionName = "Uploads";

    /// <summary>Folder under wwwroot (default <c>uploads</c>). Served at /<see cref="RelativeUrlPrefix"/>.</summary>
    public string WebRootSubfolder { get; set; } = "uploads";

    /// <summary>Leading slash, e.g. /uploads — returned in <see cref="Application.Dtos.ImageUploadResponseDto.Url"/>.</summary>
    public string RelativeUrlPrefix { get; set; } = "/uploads";

    public long MaxBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
}
