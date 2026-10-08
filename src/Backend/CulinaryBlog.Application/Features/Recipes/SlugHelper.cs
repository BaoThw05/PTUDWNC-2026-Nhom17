using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CulinaryBlog.Application.Features.Recipes;

public static partial class SlugHelper
{
    public const string ForbiddenSlug = "search";

    public static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        // 1. Chuyển chữ đ/Đ thành d/D (S-11)
        var sb = new StringBuilder(title.Length);
        foreach (var ch in title)
        {
            if (ch is 'đ' or 'Đ')
            {
                sb.Append('d');
            }
            else
            {
                sb.Append(ch);
            }
        }

        // 2. Chuẩn hóa FormD và loại bỏ dấu tiếng Việt (NonSpacingMark)
        var normalizedString = sb.ToString().Normalize(NormalizationForm.FormD);
        var cleanSb = new StringBuilder(normalizedString.Length);

        foreach (var ch in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                cleanSb.Append(ch);
            }
        }

        var text = cleanSb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant().Trim();

        // 3. Thay ký tự không phải chữ/số thành dấu gạch ngang
        text = NonAlphaNumericRegex().Replace(text, "-");

        // 4. Gộp nhiều dấu gạch ngang liên tiếp thành 1 và cắt bỏ gạch ở 2 đầu
        text = MultipleHyphensRegex().Replace(text, "-").Trim('-');

        // 5. Chặn slug là "search" (S-11)
        if (string.Equals(text, ForbiddenSlug, StringComparison.OrdinalIgnoreCase))
        {
            text = $"{ForbiddenSlug}-recipe";
        }

        return text;
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();
}
