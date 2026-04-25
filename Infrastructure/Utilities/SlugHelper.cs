using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SyriaSonsMovement.Infrastructure.Utilities;

internal static partial class SlugHelper
{
    public static string Normalize(string value)
    {
        var lower = value.Trim().ToLowerInvariant();
        var sb = new StringBuilder();
        foreach (var c in lower.Normalize(NormalizationForm.FormD))
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        var s = sb.ToString().Normalize(NormalizationForm.FormC);
        s = MyRegex().Replace(s, "-");
        s = Regex.Replace(s, "-{2,}", "-").Trim('-');
        return string.IsNullOrEmpty(s) ? "item" : s;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.CultureInvariant)]
    private static partial Regex MyRegex();
}
