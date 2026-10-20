namespace CulinaryBlog.Application.Features.Categories;

internal static class CategoryNameRules
{
    public static bool IsValid(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var trimmed = name.Trim();
        return trimmed.Length is >= 2 and <= 100 && !trimmed.Contains('<') && !trimmed.Contains('>');
    }
}
