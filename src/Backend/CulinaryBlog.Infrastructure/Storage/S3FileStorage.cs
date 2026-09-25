using Amazon.S3;
using Amazon.S3.Model;
using CulinaryBlog.Application.Abstractions;

namespace CulinaryBlog.Infrastructure.Storage;

public sealed class S3FileStorage : IFileStorage
{
    private readonly IAmazonS3 _client;
    private readonly S3StorageOptions _options;
    private readonly string _publicBaseUrl;

    public S3FileStorage(IAmazonS3 client, FileStorageOptions options)
    {
        _client = client;
        _options = options.S3;
        _publicBaseUrl = options.PublicBaseUrl;
    }

    public async Task UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = StorageKey.Normalize(key),
            InputStream = content,
            ContentType = contentType,
        }, cancellationToken);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await _client.DeleteObjectAsync(_options.BucketName, StorageKey.Normalize(key), cancellationToken);
    }

    public async Task DeleteByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var normalizedPrefix = StorageKey.NormalizePrefix(prefix) + "/";
        string? continuationToken = null;

        do
        {
            var page = await _client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _options.BucketName,
                Prefix = normalizedPrefix,
                ContinuationToken = continuationToken,
            }, cancellationToken);

            if (page.S3Objects?.Count > 0)
            {
                await _client.DeleteObjectsAsync(new DeleteObjectsRequest
                {
                    BucketName = _options.BucketName,
                    Objects = page.S3Objects.Select(item => new KeyVersion { Key = item.Key }).ToList(),
                }, cancellationToken);
            }

            continuationToken = page.IsTruncated == true ? page.NextContinuationToken : null;
        } while (continuationToken is not null);
    }

    public string GetPublicUrl(string key) => StorageKey.PublicUrl(_publicBaseUrl, key);
}
