using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.RecipeImages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;

namespace CulinaryBlog.UnitTests.RecipeImages;

public sealed class RecipeImageResizeJobTests
{
    [Fact]
    public void ProcessingStatusMigration_IsDiscovered()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .Options;
        using var db = new AppDbContext(options);

        Assert.Contains("20261007120000_AddRecipeImageProcessingStatus", db.Database.GetMigrations());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesCenteredThumbnailAndMediumWebp()
    {
        var image = new RecipeImage
        {
            Id = Guid.NewGuid(),
            RecipeId = Guid.NewGuid(),
            OriginalKey = "recipes/a/image/original.png"
        };
        var db = new TestDb(image);
        var storage = new TestStorage();
        storage.Files[image.OriginalKey] = CreatePng(1200, 600);
        var job = new RecipeImageResizeJob(db, storage, NullLogger<RecipeImageResizeJob>.Instance);

        await job.ExecuteAsync(image.Id);

        Assert.Equal("recipes/a/image/thumb.webp", image.ThumbnailKey);
        Assert.Equal("recipes/a/image/medium.webp", image.MediumKey);
        Assert.False(image.IsProcessingFailed);
        Assert.Equal(1, db.SaveCount);
        AssertDimensions(storage.Files[image.ThumbnailKey!], 300, 300);
        AssertDimensions(storage.Files[image.MediumKey!], 800, 400);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOriginalCannotBeDecoded_MarksFailureWithoutUploadingVariants()
    {
        var image = new RecipeImage
        {
            Id = Guid.NewGuid(),
            RecipeId = Guid.NewGuid(),
            OriginalKey = "recipes/a/image/original.jpg"
        };
        var db = new TestDb(image);
        var storage = new TestStorage();
        storage.Files[image.OriginalKey] = [1, 2, 3];
        var job = new RecipeImageResizeJob(db, storage, NullLogger<RecipeImageResizeJob>.Instance);

        await job.ExecuteAsync(image.Id);

        Assert.True(image.IsProcessingFailed);
        Assert.NotNull(image.ProcessingError);
        Assert.Null(image.ThumbnailKey);
        Assert.Null(image.MediumKey);
        Assert.Single(storage.Files);
        Assert.Equal(1, db.SaveCount);
    }

    private static byte[] CreatePng(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.CornflowerBlue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static void AssertDimensions(byte[] data, int width, int height)
    {
        using var bitmap = SKBitmap.Decode(data);
        Assert.NotNull(bitmap);
        Assert.Equal(width, bitmap.Width);
        Assert.Equal(height, bitmap.Height);
    }

    private sealed class TestDb(RecipeImage image) : IAppDbContext
    {
        public int SaveCount { get; private set; }
        public IQueryable<Category> Categories => Enumerable.Empty<Category>().AsQueryable();
        public IQueryable<Recipe> Recipes => Enumerable.Empty<Recipe>().AsQueryable();
        public IQueryable<Recipe> RecipesIncludingDeleted => Recipes;
        public IQueryable<RecipeStep> RecipeSteps => Enumerable.Empty<RecipeStep>().AsQueryable();
        public IQueryable<RecipeIngredient> RecipeIngredients => Enumerable.Empty<RecipeIngredient>().AsQueryable();
        public IQueryable<RecipeImage> RecipeImages => new[] { image }.AsQueryable();
        public void Add<TEntity>(TEntity entity) where TEntity : class { }
        public void Remove<TEntity>(TEntity entity) where TEntity : class { }
        public void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : BaseEntity { }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
        public Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default) => action();
    }

    private sealed class TestStorage : IFileStorage
    {
        public Dictionary<string, byte[]> Files { get; } = [];
        public async Task UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default)
        {
            using var target = new MemoryStream();
            await content.CopyToAsync(target, cancellationToken);
            Files[key] = target.ToArray();
        }
        public Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult<Stream>(new MemoryStream(Files[key], writable: false));
        public Task DeleteAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteByPrefixAsync(string prefix, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public string GetPublicUrl(string key) => $"/media/{key}";
    }
}
