using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.UnitTests.Categories;

public sealed class CategoryQueriesTests
{
    [Fact]
    public async Task GetCategories_ReturnsOrderedCategoriesWithPublishedRecipeCounts()
    {
        var first = Category("Món chính", "mon-chinh", 1);
        var second = Category("Món khai vị", "mon-khai-vi", 1);
        var db = new TestAppDbContext(
            [first, second],
            [Recipe(first.Id, RecipeStatus.Published), Recipe(first.Id, RecipeStatus.Draft)]);

        var result = await new GetCategoriesQueryHandler(db)
            .Handle(new GetCategoriesQuery(), CancellationToken.None);

        Assert.Collection(result,
            category =>
            {
                Assert.Equal("Món chính", category.Name);
                Assert.Equal(1, category.RecipeCount);
            },
            category => Assert.Equal("Món khai vị", category.Name));
    }

    [Fact]
    public async Task GetCategoryBySlug_ReturnsOnlyPublishedRecipesWithPagination()
    {
        var category = Category("Món chính", "mon-chinh", 1);
        var publishedNewest = Recipe(category.Id, RecipeStatus.Published, publishedAt: DateTimeOffset.UtcNow);
        var publishedOldest = Recipe(category.Id, RecipeStatus.Published, publishedAt: DateTimeOffset.UtcNow.AddDays(-1));
        var db = new TestAppDbContext(
            [category],
            [publishedNewest, publishedOldest, Recipe(category.Id, RecipeStatus.Draft)]);

        var result = await new GetCategoryBySlugQueryHandler(db)
            .Handle(new GetCategoryBySlugQuery(category.Slug, Page: 1, PageSize: 1), CancellationToken.None);

        Assert.Equal(2, result.Category.RecipeCount);
        Assert.Equal(2, result.Recipes.TotalCount);
        Assert.Single(result.Recipes.Items);
        Assert.Equal(publishedNewest.Id, result.Recipes.Items[0].Id);
    }

    [Fact]
    public async Task GetCategoryBySlug_WhenSlugDoesNotExist_ThrowsCategoryNotFound()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetCategoryBySlugQueryHandler(new TestAppDbContext([], []))
                .Handle(new GetCategoryBySlugQuery("khong-ton-tai"), CancellationToken.None));

        Assert.Equal(CategoryErrorCodes.CategoryNotFound, exception.Code);
    }

    private static Category Category(string name, string slug, int orderIndex) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug,
        OrderIndex = orderIndex
    };

    private static Recipe Recipe(Guid categoryId, RecipeStatus status, DateTimeOffset? publishedAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Công thức thử nghiệm",
        Slug = Guid.NewGuid().ToString("N"),
        Description = "Mô tả thử nghiệm",
        AuthorId = "author-1",
        CategoryId = categoryId,
        Status = status,
        PublishedAt = publishedAt,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private sealed class TestAppDbContext(IEnumerable<Category> categories, IEnumerable<Recipe> recipes) : IAppDbContext
    {
        public IQueryable<Category> Categories { get; } = categories.AsQueryable();
        public IQueryable<Recipe> Recipes { get; } = recipes.AsQueryable();
        public IQueryable<RecipeStep> RecipeSteps { get; } = Enumerable.Empty<RecipeStep>().AsQueryable();
        public IQueryable<RecipeIngredient> RecipeIngredients { get; } = Enumerable.Empty<RecipeIngredient>().AsQueryable();
        public IQueryable<RecipeImage> RecipeImages { get; } = Enumerable.Empty<RecipeImage>().AsQueryable();
        public IQueryable<Recipe> RecipesIncludingDeleted => Recipes;

        public void Add<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Remove<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : BaseEntity => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
