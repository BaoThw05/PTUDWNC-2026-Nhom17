using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CulinaryBlog.IntegrationTests;

public sealed class RecipesEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PostRecipe_WithoutAuth_Returns401Unauthorized()
    {
        using var client = factory.CreateClient();

        var payload = new
        {
            Title = "Công thức không xác thực",
            Description = "Mô tả công thức khi chưa có người dùng",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 30,
            Servings = 2,
            Difficulty = Difficulty.Easy
        };

        var response = await client.PostAsJsonAsync("/api/v1/recipes", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("UNAUTHORIZED", body.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task PostRecipe_WithInvalidData_Returns422UnprocessableEntity()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "author-test-1");

        var payload = new
        {
            Title = "Ngắn", // < 5 ký tự
            Description = "Mô tả",
            PrepTimeMinutes = 0, // <= 0
            CookTimeMinutes = -1,
            Servings = 0,
            Difficulty = 999
        };

        var response = await client.PostAsJsonAsync("/api/v1/recipes", payload);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("VALIDATION_ERROR", body.RootElement.GetProperty("code").GetString());
        Assert.True(body.RootElement.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task Recipe_Lifecycle_2_08_2_09_2_10_E2E()
    {
        using var client = factory.CreateClient();
        var authorId = "author-" + Guid.NewGuid().ToString("N")[..8];
        client.DefaultRequestHeaders.Add("X-User-Id", authorId);

        // 1. [2.08] Tạo Recipe Draft đầy đủ steps, ingredients, nutrition
        var createPayload = new
        {
            Title = "Bún chả Hà Nội truyền thống",
            Description = "Thịt nướng thơm lừng ăn kèm bún và nước mắm chua ngọt đặc trưng phố cổ.",
            PrepTimeMinutes = 30,
            CookTimeMinutes = 45,
            Servings = 4,
            Difficulty = Difficulty.Medium,
            Nutrition = new
            {
                Calories = 620,
                ProteinGrams = 28.5m,
                FatGrams = 22.0m,
                CarbsGrams = 75.5m,
                FiberGrams = 3.2m,
                SugarGrams = 12.0m
            },
            Steps = new[]
            {
                new { Title = "Ướp thịt", Description = "Ướp thịt ba chỉ với hành khô, tiêu, đường và nước mắm ngon.", DurationMinutes = 20 }
            },
            Ingredients = new[]
            {
                new { Name = "Thịt ba chỉ", Quantity = (decimal?)500, Unit = "g" },
                new { Name = "Bún tươi", Quantity = (decimal?)1, Unit = "kg" }
            }
        };

        var createResponse = await client.PostAsJsonAsync("/api/v1/recipes", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdResult = await createResponse.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.NotNull(createdResult);
        var recipeId = createdResult.RootElement.GetProperty("id").GetGuid();

        // 2. [2.09] Người lạ xem bài Draft -> 404 RECIPE_NOT_FOUND (S-11)
        using (var strangerClient = factory.CreateClient())
        {
            strangerClient.DefaultRequestHeaders.Add("X-User-Id", "stranger-user-999");
            var strangerResponse = await strangerClient.GetAsync($"/api/v1/recipes/{recipeId}");
            Assert.Equal(HttpStatusCode.NotFound, strangerResponse.StatusCode);

            var strangerBody = await strangerResponse.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
            Assert.NotNull(strangerBody);
            Assert.Equal("RECIPE_NOT_FOUND", strangerBody.RootElement.GetProperty("code").GetString());
        }

        // 3. [2.09] Tác giả xem bài Draft -> 200 OK và có version, nutrition, steps, ingredients
        var getAuthorResponse = await client.GetAsync($"/api/v1/recipes/{recipeId}");
        Assert.Equal(HttpStatusCode.OK, getAuthorResponse.StatusCode);

        var recipe = await getAuthorResponse.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(recipe);
        Assert.Equal("Bún chả Hà Nội truyền thống", recipe.Title);
        Assert.Equal("bun-cha-ha-noi-truyen-thong", recipe.Slug);
        Assert.Equal(RecipeStatus.Draft, recipe.Status);
        Assert.NotNull(recipe.Nutrition);
        Assert.Equal(620, recipe.Nutrition.Calories);
        Assert.Single(recipe.Steps);
        Assert.Equal(2, recipe.Ingredients.Count);
        Assert.True(recipe.Version > 0);

        // 4. [2.09] Tác giả lấy danh sách bài của mình (/me/recipes)
        var myRecipesResponse = await client.GetAsync("/api/v1/me/recipes?status=Draft&page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, myRecipesResponse.StatusCode);

        var pagedMyRecipes = await myRecipesResponse.Content.ReadFromJsonAsync<PagedResult<RecipeDto>>(JsonOptions);
        Assert.NotNull(pagedMyRecipes);
        Assert.True(pagedMyRecipes.TotalCount >= 1);
        Assert.Contains(pagedMyRecipes.Items, r => r.Id == recipeId);

        // 5. [2.10] PUT với version = 0 -> 422 VALIDATION_ERROR (bắt buộc version > 0)
        var updateWithZeroVersion = new
        {
            Title = "Bún chả Hà Nội đặc biệt",
            Description = "Cập nhật mô tả món ăn thơm ngon hơn.",
            PrepTimeMinutes = 25,
            CookTimeMinutes = 50,
            Servings = 4,
            Difficulty = Difficulty.Hard,
            Version = 0
        };

        var putZeroVersionResponse = await client.PutAsJsonAsync($"/api/v1/recipes/{recipeId}", updateWithZeroVersion);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, putZeroVersionResponse.StatusCode);

        // 6. [2.10] Người khác sửa bài -> 403 RECIPE_FORBIDDEN
        using (var strangerClient = factory.CreateClient())
        {
            strangerClient.DefaultRequestHeaders.Add("X-User-Id", "stranger-user-999");
            var updateAsStranger = new
            {
                Title = "Bún chả Hà Nội đặc biệt",
                Description = "Cập nhật mô tả món ăn thơm ngon hơn.",
                PrepTimeMinutes = 25,
                CookTimeMinutes = 50,
                Servings = 4,
                Difficulty = Difficulty.Hard,
                Version = recipe.Version
            };

            var putStrangerResponse = await strangerClient.PutAsJsonAsync($"/api/v1/recipes/{recipeId}", updateAsStranger);
            Assert.Equal(HttpStatusCode.Forbidden, putStrangerResponse.StatusCode);

            var forbiddenBody = await putStrangerResponse.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
            Assert.NotNull(forbiddenBody);
            Assert.Equal(RecipeErrorCodes.RecipeForbidden, forbiddenBody.RootElement.GetProperty("code").GetString());
        }

        // 7. [2.10] Tác giả sửa bài với version bị lệch (stale version) -> 409 CONCURRENCY_CONFLICT
        var updateWithStaleVersion = new
        {
            Title = "Bún chả Hà Nội đặc biệt",
            Description = "Cập nhật mô tả món ăn thơm ngon hơn.",
            PrepTimeMinutes = 25,
            CookTimeMinutes = 50,
            Servings = 4,
            Difficulty = Difficulty.Hard,
            Version = recipe.Version + 99999
        };

        var putStaleResponse = await client.PutAsJsonAsync($"/api/v1/recipes/{recipeId}", updateWithStaleVersion);
        Assert.Equal(HttpStatusCode.Conflict, putStaleResponse.StatusCode);

        var conflictBody = await putStaleResponse.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.NotNull(conflictBody);
        Assert.Equal("CONCURRENCY_CONFLICT", conflictBody.RootElement.GetProperty("code").GetString());

        // 8. [2.10] Tác giả sửa bài thành công với version đúng -> 204 NoContent + slug được sinh lại
        var updateValid = new
        {
            Title = "Bún chả Hà Nội gia truyền",
            Description = "Bún chả gia truyền ba đời phố cổ thơm lừng giòn rụm.",
            PrepTimeMinutes = 25,
            CookTimeMinutes = 40,
            Servings = 5,
            Difficulty = Difficulty.Hard,
            Version = recipe.Version
        };

        var putSuccessResponse = await client.PutAsJsonAsync($"/api/v1/recipes/{recipeId}", updateValid);
        Assert.Equal(HttpStatusCode.NoContent, putSuccessResponse.StatusCode);

        // Kiểm tra sau khi cập nhật: Title mới, Slug mới (vì PublishedAt == null)
        var updatedRecipeResponse = await client.GetAsync($"/api/v1/recipes/{recipeId}");
        var updatedRecipe = await updatedRecipeResponse.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(updatedRecipe);
        Assert.Equal("Bún chả Hà Nội gia truyền", updatedRecipe.Title);
        Assert.Equal("bun-cha-ha-noi-gia-truyen", updatedRecipe.Slug);
        Assert.Equal(5, updatedRecipe.Servings);
    }
}
