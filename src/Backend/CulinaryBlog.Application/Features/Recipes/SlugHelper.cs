using System.Text.RegularExpressions;

namespace CulinaryBlog.Application.Features.Recipes;

internal static partial class SlugHelper
{
    public static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant().Trim();
        slug = NonAlphaNumericRegex().Replace(slug, "");
        slug = WhitespaceRegex().Replace(slug, "-");
        return $"{slug}-{Guid.NewGuid().ToString()[..8]}";
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
