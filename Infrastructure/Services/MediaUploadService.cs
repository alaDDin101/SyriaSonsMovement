using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Infrastructure.Options;

namespace SyriaSonsMovement.Infrastructure.Services;

public sealed class MediaUploadService : IMediaUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly UploadsOptions _options;

    public MediaUploadService(IWebHostEnvironment env, IOptions<UploadsOptions> options)
    {
        _env = env;
        _options = options.Value;
    }

    public async Task<ImageUploadResponseDto> SaveImageAsync(Stream content, string originalFileName, long contentLength, CancellationToken cancellationToken = default)
    {
        if (contentLength <= 0)
            throw new InvalidOperationException("Empty file.");
        if (contentLength > _options.MaxBytes)
            throw new InvalidOperationException($"File exceeds maximum size of {_options.MaxBytes} bytes.");

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(ext))
            throw new InvalidOperationException("File must have an extension.");
        ext = ext.ToLowerInvariant();
        var allowed = new HashSet<string>(_options.AllowedExtensions.Select(e => e.ToLowerInvariant()));
        if (!allowed.Contains(ext))
            throw new InvalidOperationException($"Extension '{ext}' is not allowed. Allowed: {string.Join(", ", _options.AllowedExtensions)}.");

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var now = DateTimeOffset.UtcNow;
        var y = now.ToString("yyyy");
        var m = now.ToString("MM");
        var safeName = $"{Guid.NewGuid():N}{ext}";
        var relativeDir = Path.Combine(_options.WebRootSubfolder, y, m);
        var absoluteDir = Path.Combine(webRoot, relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var absoluteFile = Path.Combine(absoluteDir, safeName);
        await using (var fs = new FileStream(absoluteFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, useAsync: true))
        {
            await content.CopyToAsync(fs, cancellationToken);
        }

        var prefix = _options.RelativeUrlPrefix.TrimEnd('/');
        var url = $"{prefix}/{y}/{m}/{safeName}".Replace('\\', '/');
        if (!url.StartsWith('/'))
            url = "/" + url;

        return new ImageUploadResponseDto { Url = url };
    }
}
