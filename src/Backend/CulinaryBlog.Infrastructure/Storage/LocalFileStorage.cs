using CulinaryBlog.Application.Abstractions;

namespace CulinaryBlog.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;
    private readonly string _publicBaseUrl;

    public LocalFileStorage(FileStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _rootPath = Path.GetFullPath(options.LocalRootPath);
        _publicBaseUrl = options.PublicBaseUrl;
    }

    public async Task UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        var path = GetPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var destination = File.Create(path);
        await content.CopyToAsync(destination, cancellationToken);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = GetPath(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    public Task DeleteByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var normalizedPrefix = StorageKey.NormalizePrefix(prefix);
        var directory = GetPath(normalizedPrefix);
        cancellationToken.ThrowIfCancellationRequested();

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        return Task.CompletedTask;
    }

    public string GetPublicUrl(string key) => StorageKey.PublicUrl(_publicBaseUrl, key);

    private string GetPath(string key)
    {
        var normalizedKey = StorageKey.Normalize(key);
        var path = Path.GetFullPath(Path.Combine(_rootPath, normalizedKey.Replace('/', Path.DirectorySeparatorChar)));
        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;

        if (!path.StartsWith(rootWithSeparator, StringComparison.Ordinal) && !string.Equals(path, _rootPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("Storage key vượt ra ngoài thư mục lưu trữ.", nameof(key));
        }

        return path;
    }
}
