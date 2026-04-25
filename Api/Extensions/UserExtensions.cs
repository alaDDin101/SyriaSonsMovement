using System.Security.Claims;

namespace Api.Extensions;

public static class UserExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var candidates = new[]
        {
            user.FindFirstValue(ClaimTypes.NameIdentifier),
            user.FindFirstValue("sub"),
            user.FindFirstValue("nameid"),
            user.FindFirstValue("userId"),
            user.FindFirstValue("uid"),
            user.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"),
        };

        foreach (var value in candidates)
        {
            if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var id))
                return id;
        }
        return null;
    }
}
