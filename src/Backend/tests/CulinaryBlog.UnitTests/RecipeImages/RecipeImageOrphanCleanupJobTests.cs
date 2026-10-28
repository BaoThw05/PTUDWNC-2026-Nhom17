using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.RecipeImages;
using CulinaryBlog.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;

namespace CulinaryBlog.UnitTests.RecipeImages;

public sealed class RecipeImageOrphanCleanupJobTests
{
    [Fact]
    public async Task ExecuteAsync_DeletesOnlyOldOrphansAndPreservesTrashedRecipes()
    {
        var root = Path.Combine(Path.GetTempPath(), $"culinary-orphans-{Guid.NewGuid():N}");
        try
        {
            var storage = new LocalFileStorage(new FileStorageOptions { LocalRootPath = root });
            var orphanRecipeId = Guid.NewGuid();
            var orphanImageId = Guid.NewGuid();
            var liveRecipeId = Guid.NewGuid();
            var liveImageId = Guid.NewGuid();
            var trashedRecipeId = Guid.NewGuid();
            var recentRecipeId = Guid.NewGuid();
            var old = DateTime.UtcNow.AddHours(-2);

            var orphan = await AddFileAsync(storage, root, orphanRecipeId, orphanImageId, old);
            var live = await AddFileAsync(storage, root, liveRecipeId, liveImageId, old);
            var trashed = await AddFileAsync(storage, root, trashedRecipeId, Guid.NewGuid(), old);
            var recent = await AddFileAsync(storage, root, recentRecipeId, Guid.NewGuid(), DateTime.UtcNow);

            var db = new TestDb(
                [new Recipe { Id = trashedRecipeId, IsDeleted = true }],
                [new RecipeImage { Id = liveImageId, RecipeId = liveRecipeId, OriginalKey = live }]);
            var job = new RecipeImageOrphanCleanupJob(
                db, storage, TimeProvider.System, NullLogger<RecipeImageOrphanCleanupJob>.Instance);

            await job.ExecuteAsync();

            Assert.False(File.Exists(Path.Combine(root, orphan.Replace('/', Path.DirectorySeparatorChar))));
            Assert.True(File.Exists(Path.Combine(root, live.Replace('/', Path.DirectorySeparatorChar))));
            Assert.True(File.Exists(Path.Combine(root, trashed.Replace('/', Path.DirectorySeparatorChar))));
            Assert.True(File.Exists(Path.Combine(root, recent.Replace('/', Path.DirectorySeparatorChar))));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<string> AddFileAsync(
        IFileStorage storage, string root, Guid recipeId, Guid imageId, DateTime modified)
    {
        var key = $"recipes/{recipeId}/{imageId}/original.png";
        await storage.UploadAsync(new MemoryStream([1, 2, 3]), key, "image/png");
        File.SetLastWriteTimeUtc(Path.Combine(root, key.Replace('/', Path.DirectorySeparatorChar)), modified);
        return key;
    }

    private sealed class TestDb(IEnumerable<Recipe> recipes, IEnumerable<RecipeImage> images) : IAppDbContext
    {
        public IQueryable<Category> Categories => Enumerable.Empty<Category>().AsQueryable();
        public IQueryable<Recipe> Recipes => recipes.Where(recipe => !recipe.IsDeleted).AsQueryable();
        public IQueryable<Recipe> RecipesIncludingDeleted => recipes.AsQueryable();
        public IQueryable<RecipeStep> RecipeSteps => Enumerable.Empty<RecipeStep>().AsQueryable();
        public IQueryable<RecipeIngredient> RecipeIngredients => Enumerable.Empty<RecipeIngredient>().AsQueryable();
        public IQueryable<RecipeImage> RecipeImages => images.AsQueryable();
        public IQueryable<RecipeImage> RecipeImagesIncludingDeleted => images.AsQueryable();
        public void Add<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Remove<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : BaseEntity => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
