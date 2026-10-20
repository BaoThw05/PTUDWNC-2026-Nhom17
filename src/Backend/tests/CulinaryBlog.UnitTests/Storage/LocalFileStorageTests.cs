using System.Text;
using CulinaryBlog.Infrastructure.Storage;

namespace CulinaryBlog.UnitTests.Storage;

public sealed class LocalFileStorageTests : IDisposable
{
    private readonly string _rootPath = Path.Combine(Path.GetTempPath(), $"culinary-storage-{Guid.NewGuid():N}");

    [Fact]
    public async Task UploadAsync_ThenDeleteByPrefixAsync_DeletesOnlyMatchingFiles()
    {
        var storage = CreateStorage();
        await storage.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes("first")), "recipes/a/original.jpg", "image/jpeg");
        await storage.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes("second")), "recipes/b/original.jpg", "image/jpeg");

        await storage.DeleteByPrefixAsync("recipes/a/");

        Assert.False(File.Exists(Path.Combine(_rootPath, "recipes", "a", "original.jpg")));
        Assert.True(File.Exists(Path.Combine(_rootPath, "recipes", "b", "original.jpg")));
    }

    [Fact]
    public async Task DeleteAsync_IsIdempotent()
    {
        var storage = CreateStorage();

        await storage.DeleteAsync("recipes/missing/original.jpg");

        Assert.False(File.Exists(Path.Combine(_rootPath, "recipes", "missing", "original.jpg")));
    }

    [Fact]
    public void GetPublicUrl_EscapesEveryKeySegment()
    {
        var storage = CreateStorage();

        var url = storage.GetPublicUrl("recipes/a/image one.jpg");

        Assert.Equal("/media/recipes/a/image%20one.jpg", url);
    }

    [Theory]
    [InlineData("../secrets.txt")]
    [InlineData("recipes/../../secrets.txt")]
    [InlineData("/absolute/path.jpg")]
    public async Task UploadAsync_RejectsUnsafeKey(string key)
    {
        var storage = CreateStorage();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.UploadAsync(new MemoryStream([1]), key, "image/jpeg"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }

    private LocalFileStorage CreateStorage() => new(new FileStorageOptions
    {
        LocalRootPath = _rootPath,
        PublicBaseUrl = "/media",
    });
}
