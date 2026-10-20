using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.IntegrationTests.Auth;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.IntegrationTests.Categories;

[Collection(PostgresCollection.Name)]
public sealed class CategoryReadEndpointsTests(PostgresApiFactory factory)
{
    [Fact]
    public async Task GetCategories_GuestSeesOnlyPublishedRecipeCount()
    {
        var category = await SeedCategoryAndRecipesAsync();
        using var guest = factory.CreateClient();

        var response = await guest.GetAsync("/api/v1/categories");
        var categories = await response.Content.ReadFromJsonAsync<CategoryResponse[]>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(categories);
        Assert.Equal(1, Assert.Single(categories, item => item.Id == category.Id).RecipeCount);
    }

    [Fact]
    public async Task GetCategoryBySlug_GuestAndAuthorSeeOnlyAllowedRecipes()
    {
        var category = await SeedCategoryAndRecipesAsync();
        using var guest = factory.CreateClient();
        using var author = CreateClient(category.AuthorId);

        var guestResponse = await guest.GetAsync($"/api/v1/categories/{category.Slug}?page=1&pageSize=1");
        var guestDetail = await guestResponse.Content.ReadFromJsonAsync<CategoryDetailResponse>();
        var authorResponse = await author.GetAsync($"/api/v1/categories/{category.Slug}");
        var authorDetail = await authorResponse.Content.ReadFromJsonAsync<CategoryDetailResponse>();

        Assert.Equal(HttpStatusCode.OK, guestResponse.StatusCode);
        Assert.NotNull(guestDetail);
        Assert.Equal(1, guestDetail.Category.RecipeCount);
        Assert.Equal(1, guestDetail.Recipes.TotalCount);
        Assert.Equal([category.PublishedId], guestDetail.Recipes.Items.Select(item => item.Id));
        Assert.Equal(HttpStatusCode.OK, authorResponse.StatusCode);
        Assert.NotNull(authorDetail);
        Assert.Equal(2, authorDetail.Recipes.TotalCount);
        Assert.Equal(12, authorDetail.Recipes.PageSize);
        Assert.Contains(authorDetail.Recipes.Items, item => item.Id == category.PublishedId);
        Assert.Contains(authorDetail.Recipes.Items, item => item.Id == category.OwnDraftId);
    }

    [Fact]
    public async Task GetCategoryBySlug_InvalidInputReturnsProblemDetails()
    {
        using var guest = factory.CreateClient();

        var missing = await guest.GetAsync("/api/v1/categories/khong-ton-tai");
        var invalidPage = await guest.GetAsync("/api/v1/categories/mon-chinh?page=0");

        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("CATEGORY_NOT_FOUND", await AuthApi.ReadErrorCodeAsync(missing));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidPage.StatusCode);
        Assert.Equal("VALIDATION_ERROR", await AuthApi.ReadErrorCodeAsync(invalidPage));
    }

    private async Task<SeededCategory> SeedCategoryAndRecipesAsync()
    {
        var categoryId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var publishedId = Guid.NewGuid();
        var ownDraftId = Guid.NewGuid();
        var slug = $"category-test-{categoryId:N}";
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Categories.Add(new Category
        {
            Id = categoryId,
            Name = $"Danh mục thử {categoryId:N}",
            Slug = slug,
            OrderIndex = 100
        });
        db.Recipes.AddRange(
            Recipe(publishedId, categoryId, authorId, RecipeStatus.Published),
            Recipe(ownDraftId, categoryId, authorId, RecipeStatus.Draft),
            Recipe(Guid.NewGuid(), categoryId, Guid.NewGuid(), RecipeStatus.Draft),
            Recipe(Guid.NewGuid(), categoryId, authorId, RecipeStatus.Published, isDeleted: true));
        await db.SaveChangesAsync();
        return new SeededCategory(categoryId, slug, authorId, publishedId, ownDraftId);
    }

    private static Recipe Recipe(Guid id, Guid categoryId, Guid authorId, RecipeStatus status, bool isDeleted = false) => new()
    {
        Id = id,
        Title = $"Công thức thử {id:N}",
        Slug = $"recipe-test-{id:N}",
        Description = "Công thức dùng kiểm thử API danh mục.",
        AuthorId = authorId.ToString(),
        CategoryId = categoryId,
        Status = status,
        PublishedAt = status == RecipeStatus.Published ? DateTimeOffset.UtcNow : null,
        CreatedAt = DateTimeOffset.UtcNow,
        PrepTimeMinutes = 10,
        CookTimeMinutes = 10,
        Servings = 2,
        Difficulty = Difficulty.Easy,
        IsDeleted = isDeleted
    };

    private HttpClient CreateClient(Guid userId)
    {
        var client = factory.CreateClient();
        var issuer = factory.Services.GetRequiredService<IAccessTokenIssuer>();
        var user = new UserAccount(userId, "category-read@example.com", "category-read", "Category Read",
            null, true, DateTimeOffset.UtcNow, [Roles.Author]);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", issuer.Issue(user).Value);
        return client;
    }

    private sealed record SeededCategory(Guid Id, string Slug, Guid AuthorId, Guid PublishedId, Guid OwnDraftId);
    private sealed record CategoryResponse(Guid Id, int RecipeCount);
    private sealed record CategoryRecipeResponse(Guid Id);
    private sealed record CategoryPageResponse(IReadOnlyList<CategoryRecipeResponse> Items, int PageSize, int TotalCount);
    private sealed record CategoryDetailResponse(CategoryResponse Category, CategoryPageResponse Recipes);
}
