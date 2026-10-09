using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.UnitTests.Recipes;

public sealed class RecipeStepsAndIngredientsTests
{
    private static readonly Guid AuthorId = Guid.NewGuid();
    private static readonly Guid StrangerId = Guid.NewGuid();
    private readonly TestCurrentUser _currentUser = new();
    private readonly RecipeAuthorizationHandler _authHandler;

    public RecipeStepsAndIngredientsTests()
    {
        _authHandler = new RecipeAuthorizationHandler(_currentUser);
    }

    [Fact]
    public async Task AddStep_WhenAuthor_AddsWithContinuousStepNumber()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString() };
        var step1 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipe.Id, StepNumber = 1, Description = "B1" };
        var db = new FakeAppDbContext([recipe], [step1]);
        var handler = new AddRecipeStepCommandHandler(db, _authHandler);

        var step2Id = await handler.Handle(new AddRecipeStepCommand
        {
            RecipeId = recipe.Id,
            Title = "Bước 2",
            Description = "Nấu nước dùng",
            DurationMinutes = 30
        }, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, step2Id);
        var addedStep = db.RecipeSteps.First(s => s.Id == step2Id);
        Assert.Equal(2, addedStep.StepNumber);
        Assert.NotNull(recipe.UpdatedAt);
    }

    [Fact]
    public async Task AddStep_WhenStranger_ThrowsForbidden()
    {
        _currentUser.UserId = StrangerId;
        var recipe = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString() };
        var db = new FakeAppDbContext([recipe]);
        var handler = new AddRecipeStepCommandHandler(db, _authHandler);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new AddRecipeStepCommand
            {
                RecipeId = recipe.Id,
                Description = "Bước lạ"
            }, CancellationToken.None));

        Assert.Equal(RecipeErrorCodes.RecipeForbidden, ex.Code);
    }

    [Fact]
    public async Task UpdateStep_WhenAuthor_UpdatesDataAndRecipeTimestamp()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString() };
        var step = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipe.Id, StepNumber = 1, Description = "Cũ" };
        var db = new FakeAppDbContext([recipe], [step]);
        var handler = new UpdateRecipeStepCommandHandler(db, _authHandler);

        await handler.Handle(new UpdateRecipeStepCommand
        {
            RecipeId = recipe.Id,
            StepId = step.Id,
            Title = "Tiêu đề mới",
            Description = "Mô tả mới",
            DurationMinutes = 15
        }, CancellationToken.None);

        Assert.Equal("Tiêu đề mới", step.Title);
        Assert.Equal("Mô tả mới", step.Description);
        Assert.Equal(15, step.DurationMinutes);
        Assert.NotNull(recipe.UpdatedAt);
    }

    [Fact]
    public async Task DeleteStep_WhenPublishedAndLastStep_ThrowsPublishIncomplete()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorId.ToString(),
            Status = RecipeStatus.Published
        };
        var onlyStep = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipe.Id, StepNumber = 1, Description = "Duy nhất" };
        var db = new FakeAppDbContext([recipe], [onlyStep]);
        var handler = new DeleteRecipeStepCommandHandler(db, _authHandler);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new DeleteRecipeStepCommand(recipe.Id, onlyStep.Id), CancellationToken.None));

        Assert.Equal(RecipeErrorCodes.PublishIncomplete, ex.Code);
    }

    [Fact]
    public async Task DeleteStep_WhenMiddleStepDeleted_RenumbersRemainingStepsContinuously()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString(), Status = RecipeStatus.Draft };
        var step1 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipe.Id, StepNumber = 1, Description = "B1" };
        var step2 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipe.Id, StepNumber = 2, Description = "B2" };
        var step3 = new RecipeStep { Id = Guid.NewGuid(), RecipeId = recipe.Id, StepNumber = 3, Description = "B3" };
        var db = new FakeAppDbContext([recipe], [step1, step2, step3]);
        var handler = new DeleteRecipeStepCommandHandler(db, _authHandler);

        await handler.Handle(new DeleteRecipeStepCommand(recipe.Id, step2.Id), CancellationToken.None);

        Assert.DoesNotContain(db.RecipeSteps, s => s.Id == step2.Id);
        var remaining = db.RecipeSteps.Where(s => s.RecipeId == recipe.Id).OrderBy(s => s.StepNumber).ToList();
        Assert.Equal(2, remaining.Count);
        Assert.Equal(1, remaining[0].StepNumber);
        Assert.Equal(2, remaining[1].StepNumber);
        Assert.Equal(step3.Id, remaining[1].Id);
    }

    [Fact]
    public async Task AddIngredient_AllowsNullQuantity_AndAssignsOrderIndex()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString() };
        var ing1 = new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = recipe.Id, Name = "Muối", OrderIndex = 0 };
        var db = new FakeAppDbContext([recipe], null, [ing1]);
        var handler = new AddRecipeIngredientCommandHandler(db, _authHandler);

        var ing2Id = await handler.Handle(new AddRecipeIngredientCommand
        {
            RecipeId = recipe.Id,
            Name = "Tiêu xay",
            Quantity = null, // Hợp lệ theo SRS
            Unit = "chút"
        }, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, ing2Id);
        var added = db.RecipeIngredients.First(i => i.Id == ing2Id);
        Assert.Null(added.Quantity);
        Assert.Equal(1, added.OrderIndex);
    }

    [Fact]
    public async Task UpdateIngredient_UpdatesFieldsCorrectly()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString() };
        var ing = new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = recipe.Id, Name = "Đường", Quantity = 10, Unit = "g", OrderIndex = 0 };
        var db = new FakeAppDbContext([recipe], null, [ing]);
        var handler = new UpdateRecipeIngredientCommandHandler(db, _authHandler);

        await handler.Handle(new UpdateRecipeIngredientCommand
        {
            RecipeId = recipe.Id,
            IngredientId = ing.Id,
            Name = "Đường phèn",
            Quantity = 20,
            Unit = "g"
        }, CancellationToken.None);

        Assert.Equal("Đường phèn", ing.Name);
        Assert.Equal(20, ing.Quantity);
        Assert.NotNull(recipe.UpdatedAt);
    }

    [Fact]
    public async Task DeleteIngredient_RenumbersRemainingOrderIndices()
    {
        _currentUser.UserId = AuthorId;
        var recipe = new Recipe { Id = Guid.NewGuid(), AuthorId = AuthorId.ToString() };
        var ing0 = new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = recipe.Id, Name = "Nước mắm", OrderIndex = 0 };
        var ing1 = new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = recipe.Id, Name = "Hành tím", OrderIndex = 1 };
        var ing2 = new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = recipe.Id, Name = "Tỏi", OrderIndex = 2 };
        var db = new FakeAppDbContext([recipe], null, [ing0, ing1, ing2]);
        var handler = new DeleteRecipeIngredientCommandHandler(db, _authHandler);

        await handler.Handle(new DeleteRecipeIngredientCommand(recipe.Id, ing1.Id), CancellationToken.None);

        Assert.DoesNotContain(db.RecipeIngredients, i => i.Id == ing1.Id);
        var remaining = db.RecipeIngredients.Where(i => i.RecipeId == recipe.Id).OrderBy(i => i.OrderIndex).ToList();
        Assert.Equal(2, remaining.Count);
        Assert.Equal(0, remaining[0].OrderIndex);
        Assert.Equal(1, remaining[1].OrderIndex);
        Assert.Equal(ing2.Id, remaining[1].Id);
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

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Recipe r) _recipes.Add(r);
            if (entity is RecipeStep s) _steps.Add(s);
            if (entity is RecipeIngredient i) _ingredients.Add(i);
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Recipe r) _recipes.Remove(r);
            if (entity is RecipeStep s) _steps.Remove(s);
            if (entity is RecipeIngredient i) _ingredients.Remove(i);
        }

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
