namespace CulinaryBlog.Application.Abstractions;

/// <summary>Lưu file theo key trung lập với nhà cung cấp.</summary>
public interface IFileStorage
{
    Task UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    Task DeleteByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredFile>> ListFilesAsync(string prefix, CancellationToken cancellationToken = default);

    string GetPublicUrl(string key);
}

public sealed record StoredFile(string Key, DateTimeOffset LastModified);
