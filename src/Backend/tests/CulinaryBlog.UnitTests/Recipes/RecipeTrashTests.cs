using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace CulinaryBlog.UnitTests.Recipes;

public sealed class RecipeTrashTests
{
    private static readonly Guid AuthorId = Guid.NewGuid();
    private static readonly Guid StrangerId = Guid.NewGuid();
    private readonly TestCurrentUser _currentUser = new();
    private readonly RecipeAuthorizationHandler _authHandler;
    private readonly FakeCacheInvalidator _cacheInvalidator = new();

    public RecipeTrashTests()
    {
        _authHandler = new RecipeAuthorizationHandler(_currentUser);
    }

    [Fact]
    public async Task Delete_WhenAuthor_SoftDeletesAndInvalidatesCache()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString(), IsDeleted = false };
        var db = new FakeAppDbContext([recipe]);
        var handler = new DeleteRecipeCommandHandler(
            db, _authHandler, _cacheInvalidator, NullLogger<DeleteRecipeCommandHandler>.Instance);

        await handler.Handle(new DeleteRecipeCommand(recipe.Id), CancellationToken.None);

        Assert.True(recipe.IsDeleted);
        Assert.NotNull(recipe.DeletedAt);
        Assert.NotNull(recipe.UpdatedAt);
        Assert.Contains("recipes", _cacheInvalidator.InvalidatedTags);
        Assert.Contains($"recipe:{recipe.Id}", _cacheInvalidator.InvalidatedTags);
    }

    [Fact]
    public async Task Delete_WhenStranger_ThrowsForbidden()
    {
        _currentUser.UserId = StrangerId;
        var recipe = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString() };
        var db = new FakeAppDbContext([recipe]);
        var handler = new DeleteRecipeCommandHandler(
            db, _authHandler, _cacheInvalidator, NullLogger<DeleteRecipeCommandHandler>.Instance);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new DeleteRecipeCommand(recipe.Id), CancellationToken.None));

        Assert.Equal(RecipeErrorCodes.RecipeForbidden, ex.Code);
    }

    [Fact]
    public async Task Restore_WhenAuthor_RestoresAndClearsDeletedAt()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-5),
            Slug = "mon-chay-ngon"
        };
        var db = new FakeAppDbContext(deletedRecipes: [recipe]);
        var handler = new RestoreRecipeCommandHandler(
            db, _authHandler, _cacheInvalidator, NullLogger<RestoreRecipeCommandHandler>.Instance);

        await handler.Handle(new RestoreRecipeCommand(recipe.Id), CancellationToken.None);

        Assert.False(recipe.IsDeleted);
        Assert.Null(recipe.DeletedAt);
        Assert.NotNull(recipe.UpdatedAt);
        Assert.Equal("mon-chay-ngon", recipe.Slug);
        Assert.Contains("recipes", _cacheInvalidator.InvalidatedTags);
    }

    [Fact]
    public async Task Restore_WhenSlugTaken_AppendsSuffixToPreventCollision()
    {
        _currentUser.UserId = AuthorId;
        var existingActiveRecipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Slug = "mon-chay-ngon",
            IsDeleted = false
        };
        var deletedRecipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-2),
            Slug = "mon-chay-ngon"
        };

        var db = new FakeAppDbContext(
            activeRecipes: [existingActiveRecipe],
            deletedRecipes: [deletedRecipe]);
        var handler = new RestoreRecipeCommandHandler(
            db, _authHandler, _cacheInvalidator, NullLogger<RestoreRecipeCommandHandler>.Instance);

        await handler.Handle(new RestoreRecipeCommand(deletedRecipe.Id), CancellationToken.None);

        Assert.False(deletedRecipe.IsDeleted);
        Assert.Equal("mon-chay-ngon-2", deletedRecipe.Slug);
    }

    private sealed class FakeAppDbContext(
        IEnumerable<Recipe>? activeRecipes = null,
        IEnumerable<Recipe>? deletedRecipes = null) : IAppDbContext
    {
        private readonly List<Recipe> _active = [.. (activeRecipes ?? [])];
        private readonly List<Recipe> _deleted = [.. (deletedRecipes ?? [])];

        public IQueryable<Category> Categories => Enumerable.Empty<Category>().AsQueryable();
        public IQueryable<Recipe> Recipes => _active.AsQueryable();
        public IQueryable<RecipeStep> RecipeSteps => Enumerable.Empty<RecipeStep>().AsQueryable();
        public IQueryable<RecipeIngredient> RecipeIngredients => Enumerable.Empty<RecipeIngredient>().AsQueryable();
        public IQueryable<Recipe> RecipesIncludingDeleted => _active.Concat(_deleted).AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class { }
        public void Remove<TEntity>(TEntity entity) where TEntity : class { }
        public void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : CulinaryBlog.Domain.Common.BaseEntity { }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class FakeCacheInvalidator : ICacheInvalidator
    {
        public List<string> InvalidatedTags { get; } = [];

        public Task InvalidateAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
        {
            InvalidatedTags.AddRange(tags);
            return Task.CompletedTask;
        }
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; set; }
        public string? IpAddress => "127.0.0.1";
        public string? Role { get; set; }
        public bool IsInRole(string role) => string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
    }
}
