using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.IntegrationTests.Auth;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.IntegrationTests.Categories;

[Collection(PostgresCollection.Name)]
public sealed class CategoryEndpointsTests(PostgresApiFactory factory)
{
    [Fact]
    public async Task Admin_CanCreateUpdateAndDeleteEmptyCategory()
    {
        using var admin = CreateClient(Roles.Admin);
        var created = await admin.PostAsJsonAsync("/api/v1/categories", new
        {
            name = $"Danh mục thử {Guid.NewGuid():N}",
            orderIndex = 50,
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var category = await created.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(category);
        Assert.EndsWith($"/api/v1/categories/{category.Slug}", created.Headers.Location?.ToString());

        var updated = await admin.PutAsJsonAsync($"/api/v1/categories/{category.Id}", new
        {
            name = $"Danh mục đã sửa {Guid.NewGuid():N}",
            orderIndex = 51,
        });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var updatedCategory = await updated.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(updatedCategory);
        Assert.Equal(category.Slug, updatedCategory.Slug);
        Assert.Equal(51, updatedCategory.OrderIndex);

        var deleted = await admin.DeleteAsync($"/api/v1/categories/{category.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async Task Author_CannotManageCategories()
    {
        using var author = CreateClient(Roles.Author);

        var created = await author.PostAsJsonAsync("/api/v1/categories", new { name = "Không hợp lệ" });
        var updated = await author.PutAsJsonAsync($"/api/v1/categories/{Guid.NewGuid()}", new { name = "Không hợp lệ" });
        var deleted = await author.DeleteAsync($"/api/v1/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, created.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, updated.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, deleted.StatusCode);
    }

    [Fact]
    public async Task DuplicateNameIgnoringCase_Returns409WithCode()
    {
        using var admin = CreateClient(Roles.Admin);
        var name = $"Danh mục thử {Guid.NewGuid():N}";

        var first = await admin.PostAsJsonAsync("/api/v1/categories", new { name });
        var duplicate = await admin.PostAsJsonAsync("/api/v1/categories", new { name = name.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("CATEGORY_NAME_EXISTS", await AuthApi.ReadErrorCodeAsync(duplicate));
    }

    [Fact]
    public async Task DeleteCategory_WithRecipeInTrash_Returns409AndRecipeCount()
    {
        using var admin = CreateClient(Roles.Admin);
        using var author = CreateClient(Roles.Author);
        var created = await admin.PostAsJsonAsync("/api/v1/categories", new
        {
            name = $"Danh mục thử {Guid.NewGuid():N}",
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var category = await created.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(category);

        var recipe = await author.PostAsJsonAsync("/api/v1/recipes", new
        {
            title = $"Công thức thử {Guid.NewGuid():N}",
            description = "Công thức để kiểm tra xóa danh mục",
            prepTimeMinutes = 10,
            cookTimeMinutes = 10,
            servings = 2,
            difficulty = 0,
            categoryId = category.Id,
        });
        Assert.Equal(HttpStatusCode.Created, recipe.StatusCode);
        var createdRecipe = await recipe.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(createdRecipe);
        var movedToTrash = await author.DeleteAsync($"/api/v1/recipes/{createdRecipe.Id}");
        Assert.Equal(HttpStatusCode.NoContent, movedToTrash.StatusCode);

        var deleted = await admin.DeleteAsync($"/api/v1/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.Conflict, deleted.StatusCode);
        Assert.Equal("CATEGORY_DELETE_HAS_RECIPES", await AuthApi.ReadErrorCodeAsync(deleted));
        Assert.Contains("1 công thức", await deleted.Content.ReadAsStringAsync());
    }

    private HttpClient CreateClient(string role)
    {
        var client = factory.CreateClient();
        var issuer = factory.Services.GetRequiredService<IAccessTokenIssuer>();
        var user = new UserAccount(Guid.NewGuid(), "category-test@example.com", "category-test", "Category Test",
            null, true, DateTimeOffset.UtcNow, [role]);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", issuer.Issue(user).Value);
        return client;
    }

    private sealed record CategoryResponse(Guid Id, string Slug, int OrderIndex);
    private sealed record RecipeResponse(Guid Id);
}
