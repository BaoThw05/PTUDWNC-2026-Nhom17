namespace CulinaryBlog.Infrastructure.Storage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string Provider { get; init; } = "Local";

    public string PublicBaseUrl { get; init; } = "/media";

    public string LocalRootPath { get; init; } = "storage";

    public S3StorageOptions S3 { get; init; } = new();
}

public sealed class S3StorageOptions
{
    public string ServiceUrl { get; init; } = string.Empty;

    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public string BucketName { get; init; } = string.Empty;
}
