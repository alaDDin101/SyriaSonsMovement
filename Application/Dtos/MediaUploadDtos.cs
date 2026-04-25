namespace SyriaSonsMovement.Application.Dtos;

public sealed class ImageUploadResponseDto
{
    /// <summary>Root-relative path; prefix with API origin in the browser (e.g. https://api.host/uploads/...).</summary>
    public required string Url { get; init; }
}
