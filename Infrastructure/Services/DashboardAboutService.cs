using Microsoft.EntityFrameworkCore;
using SyriaSonsMovement.Application.Contracts;
using SyriaSonsMovement.Application.Dtos;
using SyriaSonsMovement.Domain;
using SyriaSonsMovement.Domain.Entities;
using SyriaSonsMovement.Infrastructure.Persistence;

namespace SyriaSonsMovement.Infrastructure.Services;

internal sealed class DashboardAboutService : IDashboardAboutService
{
    private readonly ApplicationDbContext _db;

    public DashboardAboutService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AboutUsSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var e = await _db.AboutUsSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == AboutUsIds.Singleton, cancellationToken);
        if (e == null)
            throw new InvalidOperationException("About us settings row is missing; run database migrations and seed.");
        return Map(e);
    }

    public async Task<AboutUsSettingsDto> UpsertAsync(AboutUsUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var e = await _db.AboutUsSettings.FirstOrDefaultAsync(x => x.Id == AboutUsIds.Singleton, cancellationToken);
        if (e == null)
        {
            e = new AboutUsSettings { Id = AboutUsIds.Singleton };
            _db.AboutUsSettings.Add(e);
        }

        e.Title = dto.Title.Trim();
        e.LeadText = string.IsNullOrWhiteSpace(dto.LeadText) ? null : dto.LeadText.Trim();
        e.BodyHtml = dto.BodyHtml.Trim();
        e.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        e.IsVisible = dto.IsVisible;
        e.SectionBackgroundColor = NormalizeColor(dto.SectionBackgroundColor);
        e.CardBackgroundColor = NormalizeColor(dto.CardBackgroundColor);
        e.AccentColor = NormalizeColor(dto.AccentColor);
        e.HeadingTextColor = NormalizeColor(dto.HeadingTextColor);
        e.BodyTextColor = NormalizeColor(dto.BodyTextColor);
        e.MutedTextColor = NormalizeColor(dto.MutedTextColor);
        e.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return Map(e);
    }

    private static string? NormalizeColor(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return null;
        var t = s.Trim();
        return t.Length > 64 ? t[..64] : t;
    }

    private static AboutUsSettingsDto Map(AboutUsSettings e) => new()
    {
        Title = e.Title,
        LeadText = e.LeadText,
        BodyHtml = e.BodyHtml,
        ImageUrl = e.ImageUrl,
        IsVisible = e.IsVisible,
        SectionBackgroundColor = e.SectionBackgroundColor,
        CardBackgroundColor = e.CardBackgroundColor,
        AccentColor = e.AccentColor,
        HeadingTextColor = e.HeadingTextColor,
        BodyTextColor = e.BodyTextColor,
        MutedTextColor = e.MutedTextColor,
        UpdatedAt = e.UpdatedAt,
    };
}
