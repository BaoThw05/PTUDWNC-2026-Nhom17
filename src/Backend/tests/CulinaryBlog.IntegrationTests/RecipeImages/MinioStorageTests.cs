using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CulinaryBlog.Infrastructure.Storage;
using DotNet.Testcontainers.Builders;

namespace CulinaryBlog.IntegrationTests.RecipeImages;

public sealed class MinioStorageTests
{
    [Fact]
    public async Task UploadReadAndDeletePrefix_UsesRealMinio()
    {
        await using var minio = new ContainerBuilder()
            .WithImage("quay.io/minio/minio@sha256:14cea493d9a34af32f524e538b8346cf79f3321eff8e708c1e2960462bd8936e")
            .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
            .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
            .WithCommand("server", "/data")
            .WithPortBinding(9000, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request
                .ForPort(9000)
                .ForPath("/minio/health/live")))
            .Build();
        await minio.StartAsync();

        var bucket = $"recipe-images-{Guid.NewGuid():N}";
        var endpoint = $"http://{minio.Hostname}:{minio.GetMappedPublicPort(9000)}";
        using var client = new AmazonS3Client(
            new BasicAWSCredentials("minioadmin", "minioadmin"),
            new AmazonS3Config { ServiceURL = endpoint, ForcePathStyle = true });
        await client.PutBucketAsync(bucket);

        var storage = new S3FileStorage(client, new FileStorageOptions
        {
            Provider = "S3",
            S3 = new S3StorageOptions { BucketName = bucket },
        });
        var prefix = $"recipes/{Guid.NewGuid():N}/{Guid.NewGuid():N}";
        var original = $"{prefix}/original.png";
        var thumbnail = $"{prefix}/thumb.webp";
        var sibling = $"{prefix}-other/original.png";
        var bytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jR5kAAAAASUVORK5CYII=");

        await storage.UploadAsync(new MemoryStream(bytes), original, "image/png");
        await storage.UploadAsync(new MemoryStream(bytes), thumbnail, "image/webp");
        await storage.UploadAsync(new MemoryStream(bytes), sibling, "image/png");
        await using (var read = await storage.OpenReadAsync(original))
        {
            using var copy = new MemoryStream();
            await read.CopyToAsync(copy);
            Assert.Equal(bytes, copy.ToArray());
        }

        await storage.DeleteByPrefixAsync(prefix);
        var remaining = await client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = bucket,
            Prefix = prefix + "/",
        });
        Assert.Empty(remaining.S3Objects ?? []);
        await using var untouched = await storage.OpenReadAsync(sibling);
        Assert.Equal(bytes.Length, untouched.Length);
    }
}
