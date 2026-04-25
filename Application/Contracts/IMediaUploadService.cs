using SyriaSonsMovement.Application.Dtos;

namespace SyriaSonsMovement.Application.Contracts;

public interface IMediaUploadService
{
    /// <summary>Validates and stores an image; returns a root-relative URL (e.g. /uploads/2026/04/....webp).</summary>
    Task<ImageUploadResponseDto> SaveImageAsync(Stream content, string originalFileName, long contentLength, CancellationToken cancellationToken = default);
}
