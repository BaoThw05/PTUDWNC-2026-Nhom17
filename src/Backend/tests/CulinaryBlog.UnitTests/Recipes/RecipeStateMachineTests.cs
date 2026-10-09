using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.UnitTests.Recipes;

public sealed class RecipeStateMachineTests
{
    private static readonly Guid AuthorId = Guid.NewGuid();
    private static readonly Guid StrangerId = Guid.NewGuid();
    private readonly TestCurrentUser _currentUser = new();
    private readonly RecipeAuthorizationHandler _authHandler;

    public RecipeStateMachineTests()
    {
        _authHandler = new RecipeAuthorizationHandler(_currentUser);
    }

    [Fact]
    public async Task Publish_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        _currentUser.UserId = AuthorId;
        var db = new FakeAppDbContext();
        var handler = new PublishRecipeCommandHandler(db, _authHandler);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new PublishRecipeCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal(RecipeErrorCodes.RecipeNotFound, ex.Code);
    }

    [Fact]
    public async Task Publish_WhenStrangerTriesToPublish_ThrowsForbiddenException()
    {
        _currentUser.UserId = StrangerId;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            Status = RecipeStatus.Draft
        };
        var db = new FakeAppDbContext([recipe]);
        var handler = new PublishRecipeCommandHandler(db, _authHandler);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new PublishRecipeCommand(recipe.Id), CancellationToken.None));

        Assert.Equal(RecipeErrorCodes.RecipeForbidden, ex.Code);
    }

    [Fact]
    public async Task Publish_WhenAlreadyPublished_IsIdempotent()
    {
        _currentUser.UserId = AuthorId;
        var publishedTime = DateTimeOffset.UtcNow.AddDays(-1);
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            Status = RecipeStatus.Published,
            PublishedAt = publishedTime
        };
        var db = new FakeAppDbContext([recipe]);
        var handler = new PublishRecipeCommandHandler(db, _authHandler);

        await handler.Handle(new PublishRecipeCommand(recipe.Id), CancellationToken.None);

        Assert.Equal(RecipeStatus.Published, recipe.Status);
        Assert.Equal(publishedTime, recipe.PublishedAt);
    }

    [Fact]
    public async Task Publish_WhenArchived_ThrowsValidationException()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            Status = RecipeStatus.Archived
        };
        var db = new FakeAppDbContext([recipe]);
        var handler = new PublishRecipeCommandHandler(db, _authHandler);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new PublishRecipeCommand(recipe.Id), CancellationToken.None));

        Assert.Equal(RecipeErrorCodes.InvalidStateTransition, ex.Code);
    }

    [Fact]
    public async Task Publish_WhenZeroSteps_ThrowsPublishIncomplete()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            Status = RecipeStatus.Draft
        };
        var db = new FakeAppDbContext([recipe]);
        var handler = new PublishRecipeCommandHandler(db, _authHandler);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new PublishRecipeCommand(recipe.Id), CancellationToken.None));

        Assert.Equal(RecipeErrorCodes.PublishIncomplete, ex.Code);
    }

    [Fact]
    public async Task Publish_WhenValid_SetsStatusPublishedAndPreservesPublishedAt()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            Status = RecipeStatus.Draft
        };
        var step = new RecipeStep
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            StepNumber = 1,
            Description = "Bước làm đầu tiên"
        };
        var db = new FakeAppDbContext([recipe], [step]);
        var handler = new PublishRecipeCommandHandler(db, _authHandler);

        await handler.Handle(new PublishRecipeCommand(recipe.Id), CancellationToken.None);

        Assert.Equal(RecipeStatus.Published, recipe.Status);
        Assert.NotNull(recipe.PublishedAt);
        Assert.NotNull(recipe.UpdatedAt);
    }

    [Fact]
    public async Task Unpublish_WhenPublished_TransitionsToDraft()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            Status = RecipeStatus.Published
        };
        var db = new FakeAppDbContext([recipe]);
        var handler = new UnpublishRecipeCommandHandler(db, _authHandler);

        await handler.Handle(new UnpublishRecipeCommand(recipe.Id), CancellationToken.None);

        Assert.Equal(RecipeStatus.Draft, recipe.Status);
    }

    [Fact]
    public async Task Unpublish_WhenDraft_IsIdempotent()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            Status = RecipeStatus.Draft
        };
        var db = new FakeAppDbContext([recipe]);
        var handler = new UnpublishRecipeCommandHandler(db, _authHandler);

        await handler.Handle(new UnpublishRecipeCommand(recipe.Id), CancellationToken.None);

        Assert.Equal(RecipeStatus.Draft, recipe.Status);
    }

    [Fact]
    public async Task Unpublish_WhenArchived_ThrowsValidationException()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            Status = RecipeStatus.Archived
        };
        var db = new FakeAppDbContext([recipe]);
        var handler = new UnpublishRecipeCommandHandler(db, _authHandler);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new UnpublishRecipeCommand(recipe.Id), CancellationToken.None));

        Assert.Equal(RecipeErrorCodes.InvalidStateTransition, ex.Code);
    }

    [Fact]
    public async Task Archive_WhenDraftOrPublished_TransitionsToArchived()
    {
        _currentUser.UserId = AuthorId;
        var draft = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString(), Status = RecipeStatus.Draft };
        var published = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString(), Status = RecipeStatus.Published };
        var db = new FakeAppDbContext([draft, published]);
        var handler = new ArchiveRecipeCommandHandler(db, _authHandler);

        await handler.Handle(new ArchiveRecipeCommand(draft.Id), CancellationToken.None);
        await handler.Handle(new ArchiveRecipeCommand(published.Id), CancellationToken.None);

        Assert.Equal(RecipeStatus.Archived, draft.Status);
        Assert.Equal(RecipeStatus.Archived, published.Status);
    }

    [Fact]
    public async Task Unarchive_WhenArchived_TransitionsToDraft()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            Status = RecipeStatus.Archived
        };
        var db = new FakeAppDbContext([recipe]);
        var handler = new UnarchiveRecipeCommandHandler(db, _authHandler);

        await handler.Handle(new UnarchiveRecipeCommand(recipe.Id), CancellationToken.None);

        Assert.Equal(RecipeStatus.Draft, recipe.Status);
    }

    [Fact]
    public async Task GetBySlug_WhenDraftOrArchived_ThrowsNotFound()
    {
        var draft = new Recipe { Id = Guid.NewGuid(), Slug = "mon-an-draft", Status = RecipeStatus.Draft };
        var archived = new Recipe { Id = Guid.NewGuid(), Slug = "mon-an-archived", Status = RecipeStatus.Archived };
        var db = new FakeAppDbContext([draft, archived]);
        var handler = new GetRecipeBySlugQueryHandler(db);

        var ex1 = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetRecipeBySlugQuery("mon-an-draft"), CancellationToken.None));
        var ex2 = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetRecipeBySlugQuery("mon-an-archived"), CancellationToken.None));

        Assert.Equal(RecipeErrorCodes.RecipeNotFound, ex1.Code);
        Assert.Equal(RecipeErrorCodes.RecipeNotFound, ex2.Code);
    }

    [Fact]
    public async Task GetBySlug_WhenPublished_ReturnsCompleteDto()
    {
        var published = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = "Bún bò Huế",
            Slug = "bun-bo-hue",
            Description = "Mô tả chuẩn vị cố đô",
            PrepTimeMinutes = 20,
            CookTimeMinutes = 60,
            Servings = 4,
            Difficulty = Difficulty.Medium,
            Status = RecipeStatus.Published,
            PublishedAt = DateTimeOffset.UtcNow,
            AuthorId = AuthorId.ToString(),
            Nutrition = new RecipeNutrition { Calories = 550, ProteinGrams = 30 }
        };
        var step = new RecipeStep { Id = Guid.NewGuid(), RecipeId = published.Id, StepNumber = 1, Description = "Hầm xương" };
        var ing = new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = published.Id, Name = "Bắp bò", Quantity = 500, Unit = "g", OrderIndex = 1 };
        published.Steps.Add(step);
        published.Ingredients.Add(ing);

        var db = new FakeAppDbContext([published], [step], [ing]);
        var handler = new GetRecipeBySlugQueryHandler(db);

        var result = await handler.Handle(new GetRecipeBySlugQuery("bun-bo-hue"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("bun-bo-hue", result.Slug);
        Assert.Equal(RecipeStatus.Published, result.Status);
        Assert.NotNull(result.Nutrition);
        Assert.Equal(550, result.Nutrition.Calories);
        Assert.Single(result.Steps);
        Assert.Single(result.Ingredients);
    }

    private sealed class FakeAppDbContext(
        IEnumerable<Recipe>? recipes = null,
        IEnumerable<RecipeStep>? steps = null,
        IEnumerable<RecipeIngredient>? ingredients = null) : IAppDbContext
    {
        private readonly List<Recipe> _recipes = [.. (recipes ?? [])];
        private readonly List<RecipeStep> _steps = [.. (steps ?? [])];
        private readonly List<RecipeIngredient> _ingredients = [.. (ingredients ?? [])];

        public IQueryable<Category> Categories => Enumerable.Empty<Category>().AsQueryable();
        public IQueryable<Recipe> Recipes => _recipes.AsQueryable();
        public IQueryable<RecipeStep> RecipeSteps => _steps.AsQueryable();
        public IQueryable<RecipeIngredient> RecipeIngredients => _ingredients.AsQueryable();
        public IQueryable<Recipe> RecipesIncludingDeleted => _recipes.AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class { }
        public void Remove<TEntity>(TEntity entity) where TEntity : class { }
        public void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : CulinaryBlog.Domain.Common.BaseEntity { }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; set; }
        public string? IpAddress => "127.0.0.1";
        public string? Role { get; set; }
        public bool IsInRole(string role) => string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
    }
}
