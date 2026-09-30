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
    public async Task CreateCategory_GeneratesVietnameseSlug()
    {
        var db = new TestAppDbContext();

        var result = await new CreateCategoryCommandHandler(db).Handle(new CreateCategoryCommand
        {
            Name = "Món Ăn Đặc Biệt",
            Description = "Mô tả"
        }, CancellationToken.None);

        Assert.Equal("mon-an-dac-biet", result.Slug);
        Assert.Single(db.CategoryItems);
    }

    [Fact]
    public async Task CreateCategory_WhenNameExistsIgnoringCase_ThrowsConflict()
    {
        var db = new TestAppDbContext([Category("Món chính", "mon-chinh")]);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            new CreateCategoryCommandHandler(db).Handle(new CreateCategoryCommand { Name = "mÓN CHÍNH" }, CancellationToken.None));

        Assert.Equal(CategoryErrorCodes.CategoryNameExists, exception.Code);
    }

    [Fact]
    public async Task UpdateCategory_KeepsExistingSlug()
    {
        var category = Category("Món chính", "mon-chinh");
        var db = new TestAppDbContext([category]);

        var result = await new UpdateCategoryCommandHandler(db).Handle(new UpdateCategoryCommand
        {
            Id = category.Id,
            Name = "Món chính mới",
            OrderIndex = 2
        }, CancellationToken.None);

        Assert.Equal("mon-chinh", result.Slug);
        Assert.Equal("Món chính mới", category.Name);
    }

    [Fact]
    public async Task DeleteCategory_WhenDeletedRecipeExists_ThrowsConflict()
    {
        var category = Category("Món chính", "mon-chinh");
        var db = new TestAppDbContext(
            [category],
            [Recipe(category.Id, isDeleted: true)]);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            new DeleteCategoryCommandHandler(db).Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None));

        Assert.Equal(CategoryErrorCodes.CategoryDeleteHasRecipes, exception.Code);
        Assert.Contains(category, db.CategoryItems);
    }

    [Fact]
    public async Task DeleteCategory_WhenNoRecipes_RemovesCategory()
    {
        var category = Category("Món chính", "mon-chinh");
        var db = new TestAppDbContext([category]);

        await new DeleteCategoryCommandHandler(db).Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        Assert.Empty(db.CategoryItems);
    }

    private static Category Category(string name, string slug) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug
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

    private sealed class TestAppDbContext(
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
            if (entity is Category category)
            {
                CategoryItems.Add(category);
            }
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Category category)
            {
                CategoryItems.Remove(category);
            }
        }

        public void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : BaseEntity { }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }
}
