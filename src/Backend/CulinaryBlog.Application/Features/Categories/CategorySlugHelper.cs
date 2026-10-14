using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CulinaryBlog.Application.Abstractions;

namespace CulinaryBlog.Application.Features.Categories;

internal static partial class CategorySlugHelper
{
    private const int MaxSlugLength = 120;

    public static string GenerateUniqueSlug(IAppDbContext db, string name)
    {
        var baseSlug = GenerateBaseSlug(name);
        var slug = baseSlug;
        var suffix = 2;

        while (db.Categories.Any(category => category.Slug == slug))
        {
            var suffixText = $"-{suffix++}";
            slug = $"{baseSlug[..Math.Min(baseSlug.Length, MaxSlugLength - suffixText.Length)]}{suffixText}";
        }

        return slug;
    }

    private static string GenerateBaseSlug(string value)
    {
        var normalized = value.Trim()
            .ToLowerInvariant()
            .Replace('đ', 'd')
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var slug = NonSlugCharacterRegex().Replace(builder.ToString(), "-").Trim('-');
        return slug.Length == 0 ? "danh-muc" : slug[..Math.Min(slug.Length, MaxSlugLength)];
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacterRegex();
}
