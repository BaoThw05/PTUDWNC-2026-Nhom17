using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Categories;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.UnitTests.Categories;

public sealed class CategoryQueriesTests
{
    [Fact]
    public async Task GetCategories_WhenEmpty_ReturnsEmptyList()
    {
        var result = await new GetCategoriesQueryHandler(new TestAppDbContext([], []))
            .Handle(new GetCategoriesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

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

        var result = await new GetCategoryBySlugQueryHandler(db, new TestCurrentUser())
            .Handle(new GetCategoryBySlugQuery(category.Slug, Page: 1, PageSize: 1), CancellationToken.None);

        Assert.Equal(2, result.Category.RecipeCount);
        Assert.Equal(2, result.Recipes.TotalCount);
        Assert.Single(result.Recipes.Items);
        Assert.Equal(publishedNewest.Id, result.Recipes.Items[0].Id);
    }

    [Fact]
    public async Task GetCategoryBySlug_AuthorSeesOnlyOwnDraftAndPublishedRecipes()
    {
        var category = Category("Món chính", "mon-chinh", 1);
        var authorId = Guid.NewGuid();
        var published = Recipe(category.Id, RecipeStatus.Published);
        var ownDraft = Recipe(category.Id, RecipeStatus.Draft, authorId: authorId.ToString());
        var otherDraft = Recipe(category.Id, RecipeStatus.Draft, authorId: Guid.NewGuid().ToString());
        var deletedPublished = Recipe(category.Id, RecipeStatus.Published, isDeleted: true);
        var db = new TestAppDbContext([category], [published, ownDraft, otherDraft, deletedPublished]);

        var guestResult = await new GetCategoryBySlugQueryHandler(db, new TestCurrentUser())
            .Handle(new GetCategoryBySlugQuery(category.Slug), CancellationToken.None);
        var nonAuthorResult = await new GetCategoryBySlugQueryHandler(db, new TestCurrentUser { UserId = authorId })
            .Handle(new GetCategoryBySlugQuery(category.Slug), CancellationToken.None);
        var authorResult = await new GetCategoryBySlugQueryHandler(db, new TestCurrentUser { UserId = authorId, Role = Roles.Author })
            .Handle(new GetCategoryBySlugQuery(category.Slug), CancellationToken.None);

        Assert.Equal([published.Id], guestResult.Recipes.Items.Select(item => item.Id));
        Assert.Equal([published.Id], nonAuthorResult.Recipes.Items.Select(item => item.Id));
        Assert.Equal(2, authorResult.Recipes.TotalCount);
        Assert.Contains(authorResult.Recipes.Items, item => item.Id == published.Id);
        Assert.Contains(authorResult.Recipes.Items, item => item.Id == ownDraft.Id);
        Assert.DoesNotContain(authorResult.Recipes.Items, item => item.Id == otherDraft.Id || item.Id == deletedPublished.Id);
        Assert.Equal(1, authorResult.Category.RecipeCount);
    }

    [Fact]
    public async Task GetCategoryBySlug_HugePage_ReturnsEmptyItemsWithoutOverflow()
    {
        var category = Category("Món chính", "mon-chinh", 1);
        var db = new TestAppDbContext([category], [Recipe(category.Id, RecipeStatus.Published)]);

        var result = await new GetCategoryBySlugQueryHandler(db, new TestCurrentUser())
            .Handle(new GetCategoryBySlugQuery(category.Slug, int.MaxValue, 50), CancellationToken.None);

        Assert.Equal(1, result.Recipes.TotalCount);
        Assert.Empty(result.Recipes.Items);
    }

    [Fact]
    public async Task GetCategoryBySlug_WhenSlugDoesNotExist_ThrowsCategoryNotFound()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetCategoryBySlugQueryHandler(new TestAppDbContext([], []), new TestCurrentUser())
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

    private static Recipe Recipe(
        Guid categoryId,
        RecipeStatus status,
        DateTimeOffset? publishedAt = null,
        string authorId = "author-1",
        bool isDeleted = false) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Công thức thử nghiệm",
        Slug = Guid.NewGuid().ToString("N"),
        Description = "Mô tả thử nghiệm",
        AuthorId = authorId,
        CategoryId = categoryId,
        Status = status,
        PublishedAt = publishedAt,
        CreatedAt = DateTimeOffset.UtcNow,
        IsDeleted = isDeleted
    };

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; init; }
        public string? Role { get; init; }
        public string? IpAddress => null;
        public bool IsInRole(string role) => Role == role;
    }

    private sealed class TestAppDbContext(IEnumerable<Category> categories, IEnumerable<Recipe> recipes) : IAppDbContext
    {
        public IQueryable<Category> Categories { get; } = categories.AsQueryable();
        public IQueryable<Recipe> Recipes { get; } = recipes.Where(recipe => !recipe.IsDeleted).AsQueryable();
        public IQueryable<RecipeStep> RecipeSteps { get; } = Enumerable.Empty<RecipeStep>().AsQueryable();
        public IQueryable<RecipeIngredient> RecipeIngredients { get; } = Enumerable.Empty<RecipeIngredient>().AsQueryable();
        public IQueryable<Recipe> RecipesIncludingDeleted => Recipes;

        public void Add<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void Remove<TEntity>(TEntity entity) where TEntity : class => throw new NotSupportedException();
        public void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : BaseEntity => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
