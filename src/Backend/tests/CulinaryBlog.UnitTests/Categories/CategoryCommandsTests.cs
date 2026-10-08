using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.UnitTests.Categories;

public sealed class CategoryCommandsTests
{
    [Fact]
    public async Task Create_GeneratesVietnameseSlugWithSuffixAndInvalidatesCache()
    {
        var db = new TestDb([Category("Bữa ăn", "bua-an")]);
        var cache = new TestCache();

        var created = await new CreateCategoryCommandHandler(db, cache).Handle(
            new CreateCategoryCommand { Name = "Bữa Ăn!", OrderIndex = 2 }, CancellationToken.None);

        Assert.Equal("bua-an-2", created.Slug);
        Assert.Equal(2, created.OrderIndex);
        Assert.Equal(["categories"], cache.InvalidatedTags);
    }

    [Fact]
    public async Task Create_WithExistingNameIgnoringCase_ReturnsConflict()
    {
        var db = new TestDb([Category("Món chính", "mon-chinh")]);

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            new CreateCategoryCommandHandler(db, new TestCache()).Handle(
                new CreateCategoryCommand { Name = "mÓN CHÍNH" }, CancellationToken.None));

        Assert.Equal(CategoryErrorCodes.CategoryNameExists, error.Code);
    }

    [Fact]
    public void NameValidation_RejectsWhitespaceAndHtml()
    {
        var validator = new CreateCategoryCommandValidator();

        Assert.False(validator.Validate(new CreateCategoryCommand { Name = "   " }).IsValid);
        Assert.False(validator.Validate(new CreateCategoryCommand { Name = "<b>Món</b>" }).IsValid);
    }

    [Fact]
    public async Task Update_KeepsSlugAndInvalidatesCache()
    {
        var category = Category("Món chính", "mon-chinh");
        var db = new TestDb([category]);
        var cache = new TestCache();

        var updated = await new UpdateCategoryCommandHandler(db, cache).Handle(
            new UpdateCategoryCommand { Id = category.Id, Name = "Món chính mới" }, CancellationToken.None);

        Assert.Equal("mon-chinh", updated.Slug);
        Assert.Equal("Món chính mới", category.Name);
        Assert.Equal(["categories"], cache.InvalidatedTags);
    }

    [Fact]
    public async Task Delete_WithSoftDeletedRecipe_ReturnsCountAndDoesNotInvalidateCache()
    {
        var category = Category("Món chính", "mon-chinh");
        var db = new TestDb([category], [Recipe(category.Id, true)]);
        var cache = new TestCache();

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            new DeleteCategoryCommandHandler(db, cache).Handle(
                new DeleteCategoryCommand(category.Id), CancellationToken.None));

        Assert.Equal(CategoryErrorCodes.CategoryDeleteHasRecipes, error.Code);
        Assert.Contains("1 công thức", error.Message);
        Assert.Contains(category, db.CategoryItems);
        Assert.Empty(cache.InvalidatedTags);
    }

    [Fact]
    public async Task Delete_EmptyCategory_RemovesItAndInvalidatesCache()
    {
        var category = Category("Món chính", "mon-chinh");
        var db = new TestDb([category]);
        var cache = new TestCache();

        await new DeleteCategoryCommandHandler(db, cache).Handle(
            new DeleteCategoryCommand(category.Id), CancellationToken.None);

        Assert.Empty(db.CategoryItems);
        Assert.Equal(["categories"], cache.InvalidatedTags);
    }

    private static Category Category(string name, string slug) => new()
    {
        Id = Guid.NewGuid(), Name = name, Slug = slug
    };

    private static Recipe Recipe(Guid categoryId, bool isDeleted) => new()
    {
        CategoryId = categoryId,
        Title = "Công thức thử nghiệm",
        Slug = Guid.NewGuid().ToString("N"),
        Description = "Mô tả thử nghiệm",
        AuthorId = "author-1",
        Status = RecipeStatus.Draft,
        IsDeleted = isDeleted
    };

    private sealed class TestCache : ICacheInvalidator
    {
        public List<string> InvalidatedTags { get; } = [];

        public Task InvalidateAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
        {
            InvalidatedTags.AddRange(tags);
            return Task.CompletedTask;
        }
    }

    private sealed class TestDb(
        IEnumerable<Category>? categories = null,
        IEnumerable<Recipe>? recipes = null) : IAppDbContext
    {
        public List<Category> CategoryItems { get; } = categories?.ToList() ?? [];
        private List<Recipe> RecipeItems { get; } = recipes?.ToList() ?? [];

        public IQueryable<Category> Categories => CategoryItems.AsQueryable();
        public IQueryable<Recipe> Recipes => RecipeItems.Where(recipe => !recipe.IsDeleted).AsQueryable();
        public IQueryable<Recipe> RecipesIncludingDeleted => RecipeItems.AsQueryable();
        public IQueryable<RecipeStep> RecipeSteps => Enumerable.Empty<RecipeStep>().AsQueryable();
        public IQueryable<RecipeIngredient> RecipeIngredients => Enumerable.Empty<RecipeIngredient>().AsQueryable();

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Category category) CategoryItems.Add(category);
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Category category) CategoryItems.Remove(category);
        }

        public void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : BaseEntity { }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }
}
