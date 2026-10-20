namespace CulinaryBlog.Infrastructure.Storage;

internal static class StorageKey
{
    public static string Normalize(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var normalized = key.Replace('\\', '/');
        if (normalized.StartsWith("/", StringComparison.Ordinal)
            || normalized.Length == 0
            || normalized.Split('/').Any(part => part is "." or ".." || part.Length == 0))
        {
            throw new ArgumentException("Storage key phải là đường dẫn tương đối hợp lệ.", nameof(key));
        }

        return normalized;
    }

    public static string PublicUrl(string publicBaseUrl, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publicBaseUrl);
        var escapedKey = string.Join('/', Normalize(key).Split('/').Select(Uri.EscapeDataString));
        return $"{publicBaseUrl.TrimEnd('/')}/{escapedKey}";
    }

    public static string NormalizePrefix(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        return Normalize(prefix.TrimEnd('/', '\\'));
    }
}
