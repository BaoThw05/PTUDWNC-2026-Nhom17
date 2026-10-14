using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace CulinaryBlog.UnitTests.Recipes;

public sealed class RecipeTrashCleanupJobTests
{
    private readonly TestTimeProvider _timeProvider = new(new DateTimeOffset(2026, 10, 14, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeFileStorage _fileStorage = new();

    [Fact]
    public async Task ExecuteAsync_WhenRecipesDeletedMoreThan30Days_DeletesFromDbAndStorage()
    {
        var now = _timeProvider.GetUtcNow();

        // Recipe 1: Xóa mềm cách đây 31 ngày (đã quá 30 ngày) -> Bị xóa vĩnh viễn
        var expiredDeletedRecipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = "Công thức cũ hết hạn",
            AuthorId = Guid.NewGuid().ToString(),
            IsDeleted = true,
            DeletedAt = now.AddDays(-31)
        };

        // Recipe 2: Xóa mềm cách đây 10 ngày (chưa quá 30 ngày) -> Giữ nguyên trong thùng rác
        var recentDeletedRecipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = "Công thức mới xóa 10 ngày",
            AuthorId = Guid.NewGuid().ToString(),
            IsDeleted = true,
            DeletedAt = now.AddDays(-10)
        };

        // Recipe 3: Đang hoạt động bình thường -> Giữ nguyên
        var activeRecipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = "Công thức đang hoạt động",
            AuthorId = Guid.NewGuid().ToString(),
            IsDeleted = false,
            DeletedAt = null
        };

        var db = new FakeAppDbContext([activeRecipe], [expiredDeletedRecipe, recentDeletedRecipe]);
        var job = new RecipeTrashCleanupJob(db, _fileStorage, _timeProvider, NullLogger<RecipeTrashCleanupJob>.Instance);

        var deletedCount = await job.ExecuteAsync(CancellationToken.None);

        // 1. Số lượng công thức bị xóa thật phải là 1
        Assert.Equal(1, deletedCount);

        // 2. Storage phải được gọi xóa prefix "recipes/{id}" cho expiredDeletedRecipe
        Assert.Contains($"recipes/{expiredDeletedRecipe.Id}", _fileStorage.DeletedPrefixes);
        Assert.DoesNotContain($"recipes/{recentDeletedRecipe.Id}", _fileStorage.DeletedPrefixes);
        Assert.DoesNotContain($"recipes/{activeRecipe.Id}", _fileStorage.DeletedPrefixes);

        // 3. Entity expiredDeletedRecipe phải bị gỡ khỏi CSDL
        Assert.Contains(expiredDeletedRecipe, db.RemovedEntities);
        Assert.DoesNotContain(recentDeletedRecipe, db.RemovedEntities);
        Assert.DoesNotContain(activeRecipe, db.RemovedEntities);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoExpiredRecipes_ReturnsZeroAndDoesNothing()
    {
        var now = _timeProvider.GetUtcNow();
        var recentDeletedRecipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = "Công thức mới xóa",
            AuthorId = Guid.NewGuid().ToString(),
            IsDeleted = true,
            DeletedAt = now.AddDays(-15)
        };

        var db = new FakeAppDbContext(deletedRecipes: [recentDeletedRecipe]);
        var job = new RecipeTrashCleanupJob(db, _fileStorage, _timeProvider, NullLogger<RecipeTrashCleanupJob>.Instance);

        var deletedCount = await job.ExecuteAsync(CancellationToken.None);

        Assert.Equal(0, deletedCount);
        Assert.Empty(_fileStorage.DeletedPrefixes);
        Assert.Empty(db.RemovedEntities);
    }

    private sealed class TestTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan duration) => _now += duration;
    }

    private sealed class FakeFileStorage : IFileStorage
    {
        public List<string> DeletedPrefixes { get; } = [];

        public Task UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            DeletedPrefixes.Add(prefix);
            return Task.CompletedTask;
        }

        public string GetPublicUrl(string key) => $"http://localhost:5000/{key}";
    }

    private sealed class FakeAppDbContext(
        IEnumerable<Recipe>? activeRecipes = null,
        IEnumerable<Recipe>? deletedRecipes = null) : IAppDbContext
    {
        private readonly List<Recipe> _active = [.. (activeRecipes ?? [])];
        private readonly List<Recipe> _deleted = [.. (deletedRecipes ?? [])];

        public List<object> RemovedEntities { get; } = [];

        public IQueryable<Category> Categories => Enumerable.Empty<Category>().AsQueryable();
        public IQueryable<Recipe> Recipes => _active.AsQueryable();
        public IQueryable<RecipeStep> RecipeSteps => Enumerable.Empty<RecipeStep>().AsQueryable();
        public IQueryable<RecipeIngredient> RecipeIngredients => Enumerable.Empty<RecipeIngredient>().AsQueryable();
        public IQueryable<RecipeImage> RecipeImages => Enumerable.Empty<RecipeImage>().AsQueryable();
        public IQueryable<Recipe> RecipesIncludingDeleted => _active.Concat(_deleted).AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class { }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            RemovedEntities.Add(entity);
            if (entity is Recipe r)
            {
                _active.Remove(r);
                _deleted.Remove(r);
            }
        }

        public void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : BaseEntity { }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }
}
