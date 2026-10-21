using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.IntegrationTests.Auth;
using CulinaryBlog.IntegrationTests.Infrastructure;

namespace CulinaryBlog.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class RecipesEndpointsTests(PostgresApiFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Đăng ký một tài khoản mới và trả về client đã gắn access token (thay cho header X-User-Id cũ).
    private async Task<HttpClient> CreateAuthorClientAsync()
    {
        var client = factory.CreateClient();
        var auth = await AuthApi.ReadAuthAsync(
            await AuthApi.RegisterAsync(client, $"recipe-{Guid.NewGuid():N}@culinaryblog.test"));
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }

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
        using var client = await CreateAuthorClientAsync();

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
        using var client = await CreateAuthorClientAsync();

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
        using (var strangerClient = await CreateAuthorClientAsync())
        {
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
        using (var strangerClient = await CreateAuthorClientAsync())
        {
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
            Version = recipe.Version,
            Nutrition = new
            {
                Calories = 620,
                ProteinGrams = 28.5m,
                FatGrams = 22.0m,
                CarbsGrams = 75.5m,
                FiberGrams = 3.2m,
                SugarGrams = 12.0m
            }
        };

        var putSuccessResponse = await client.PutAsJsonAsync($"/api/v1/recipes/{recipeId}", updateValid);
        Assert.Equal(HttpStatusCode.NoContent, putSuccessResponse.StatusCode);

        // Kiểm tra sau khi cập nhật: Title mới, Slug mới (vì PublishedAt == null)
        var updatedRecipeResponse = await client.GetAsync($"/api/v1/recipes/{recipeId}");
        var updatedRecipe = await updatedRecipeResponse.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(updatedRecipe);
        Assert.Equal("Bún chả Hà Nội gia truyền", updatedRecipe.Title);
        // Slug được sinh từ title mới; có thể có hậu tố -N nếu slug cùng đã tồn tại trong DB
        Assert.StartsWith("bun-cha-ha-noi-gia-truyen", updatedRecipe.Slug);
        Assert.Equal(5, updatedRecipe.Servings);

        var currentSlug = updatedRecipe.Slug;

        // 9. [2.12] Khách vãng lai gọi GET /recipes/{slug} khi bài đang là Draft -> 404 RECIPE_NOT_FOUND
        using var guestClient = factory.CreateClient();
        var getSlugWhileDraftResponse = await guestClient.GetAsync($"/api/v1/recipes/{currentSlug}");
        Assert.Equal(HttpStatusCode.NotFound, getSlugWhileDraftResponse.StatusCode);
        var notFoundSlugBody = await getSlugWhileDraftResponse.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.NotNull(notFoundSlugBody);
        Assert.Equal(RecipeErrorCodes.RecipeNotFound, notFoundSlugBody.RootElement.GetProperty("code").GetString());

        // 10. [2.11] Người lạ cố tình gọi POST /recipes/{id}/publish -> 403 RECIPE_FORBIDDEN
        using var anotherStrangerClient = await CreateAuthorClientAsync();
        var strangerPublishResponse = await anotherStrangerClient.PostAsync($"/api/v1/recipes/{recipeId}/publish", null);
        Assert.Equal(HttpStatusCode.Forbidden, strangerPublishResponse.StatusCode);

        // 11. [2.11] Tác giả publish bài viết thành công -> 204 NoContent
        var publishResponse = await client.PostAsync($"/api/v1/recipes/{recipeId}/publish", null);
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        // Kiểm tra bài đã Published và có PublishedAt
        var publishedDetailResponse = await client.GetAsync($"/api/v1/recipes/{recipeId}");
        var publishedDetail = await publishedDetailResponse.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(publishedDetail);
        Assert.Equal(RecipeStatus.Published, publishedDetail.Status);
        Assert.NotNull(publishedDetail.PublishedAt);

        // 12. [2.11] Tác giả publish lần 2 (idempotent S-11) -> 204 NoContent
        var secondPublishResponse = await client.PostAsync($"/api/v1/recipes/{recipeId}/publish", null);
        Assert.Equal(HttpStatusCode.NoContent, secondPublishResponse.StatusCode);

        // 13. [2.12] Khách vãng lai gọi GET /recipes/{slug} khi bài đã Published -> 200 OK + đầy đủ DTO
        var getSlugPublishedResponse = await guestClient.GetAsync($"/api/v1/recipes/{currentSlug}");
        Assert.Equal(HttpStatusCode.OK, getSlugPublishedResponse.StatusCode);
        var publicRecipe = await getSlugPublishedResponse.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(publicRecipe);
        Assert.Equal("Bún chả Hà Nội gia truyền", publicRecipe.Title);
        Assert.Equal(currentSlug, publicRecipe.Slug);
        Assert.Equal(RecipeStatus.Published, publicRecipe.Status);
        Assert.NotNull(publicRecipe.Nutrition);
        Assert.Equal(620, publicRecipe.Nutrition.Calories);
        Assert.Single(publicRecipe.Steps);
        Assert.Equal(2, publicRecipe.Ingredients.Count);

        // 14. [2.11] Tác giả Unpublish bài viết -> 204 NoContent
        var unpublishResponse = await client.PostAsync($"/api/v1/recipes/{recipeId}/unpublish", null);
        Assert.Equal(HttpStatusCode.NoContent, unpublishResponse.StatusCode);

        // 15. [2.12] Khách vãng lai gọi GET /recipes/{slug} sau khi bài bị Unpublish -> 404 NOT_FOUND
        var getSlugAfterUnpublishResponse = await guestClient.GetAsync($"/api/v1/recipes/{currentSlug}");
        Assert.Equal(HttpStatusCode.NotFound, getSlugAfterUnpublishResponse.StatusCode);

        // 16. [2.11] Tác giả Archive bài viết -> 204 NoContent, sau đó Unarchive về Draft -> 204 NoContent
        var archiveResponse = await client.PatchAsync($"/api/v1/recipes/{recipeId}/archive", null);
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var archivedDetailResponse = await client.GetAsync($"/api/v1/recipes/{recipeId}");
        var archivedDetail = await archivedDetailResponse.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(archivedDetail);
        Assert.Equal(RecipeStatus.Archived, archivedDetail.Status);

        var unarchiveResponse = await client.PatchAsync($"/api/v1/recipes/{recipeId}/unarchive", null);
        Assert.Equal(HttpStatusCode.NoContent, unarchiveResponse.StatusCode);

        var unarchivedDetailResponse = await client.GetAsync($"/api/v1/recipes/{recipeId}");
        var unarchivedDetail = await unarchivedDetailResponse.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(unarchivedDetail);
        Assert.Equal(RecipeStatus.Draft, unarchivedDetail.Status);

        // 17. [2.13] Thêm bước thứ 2 và sửa bước
        var addStepPayload = new
        {
            Title = "Nướng thịt",
            Description = "Nướng than hoa đến khi vàng xém hai mặt",
            DurationMinutes = 25
        };
        var addStepResponse = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", addStepPayload);
        Assert.Equal(HttpStatusCode.Created, addStepResponse.StatusCode);
        var step2Doc = await addStepResponse.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.NotNull(step2Doc);
        var step2Id = step2Doc.RootElement.GetProperty("id").GetGuid();

        var updateStepPayload = new
        {
            Title = "Nướng thịt than hoa",
            Description = "Nướng than hoa vàng giòn thơm nức mũi",
            DurationMinutes = 30
        };
        var updateStepResponse = await client.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/steps/{step2Id}", updateStepPayload);
        Assert.Equal(HttpStatusCode.NoContent, updateStepResponse.StatusCode);

        // 18. [2.13] Xóa bước và bảo vệ bước cuối khi Published
        var deleteStep2Response = await client.DeleteAsync($"/api/v1/recipes/{recipeId}/steps/{step2Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteStep2Response.StatusCode);

        // Publish lại bài viết
        await client.PostAsync($"/api/v1/recipes/{recipeId}/publish", null);

        // Lấy bước duy nhất còn lại
        var recipeWithOneStepResp = await client.GetAsync($"/api/v1/recipes/{recipeId}");
        var recipeWithOneStep = await recipeWithOneStepResp.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(recipeWithOneStep);
        Assert.Single(recipeWithOneStep.Steps);
        var lastStepId = recipeWithOneStep.Steps[0].Id;

        // Xóa bước cuối cùng khi bài đang Published -> 422 RECIPE_PUBLISH_INCOMPLETE
        var deleteLastStepResponse = await client.DeleteAsync($"/api/v1/recipes/{recipeId}/steps/{lastStepId}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, deleteLastStepResponse.StatusCode);

        // 19. [2.14] Thêm, sửa, xóa nguyên liệu độc lập
        var addIngPayload = new
        {
            Name = "Rau sống",
            Quantity = (decimal?)200,
            Unit = "g"
        };
        var addIngResponse = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/ingredients", addIngPayload);
        Assert.Equal(HttpStatusCode.Created, addIngResponse.StatusCode);
        var ingDoc = await addIngResponse.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.NotNull(ingDoc);
        var newIngId = ingDoc.RootElement.GetProperty("id").GetGuid();

        var updateIngPayload = new
        {
            Name = "Rau sống kinh giới, tía tô",
            Quantity = (decimal?)300,
            Unit = "g"
        };
        var updateIngResponse = await client.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/ingredients/{newIngId}", updateIngPayload);
        Assert.Equal(HttpStatusCode.NoContent, updateIngResponse.StatusCode);

        var deleteIngResponse = await client.DeleteAsync($"/api/v1/recipes/{recipeId}/ingredients/{newIngId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteIngResponse.StatusCode);

        // 20. [2.15] Xóa bài vào thùng rác (Soft-delete)
        var deleteRecipeResponse = await client.DeleteAsync($"/api/v1/recipes/{recipeId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRecipeResponse.StatusCode);

        // Khách gọi GET theo slug hay id đều nhận 404
        var getDeletedBySlugResp = await guestClient.GetAsync($"/api/v1/recipes/{currentSlug}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedBySlugResp.StatusCode);

        var getDeletedByIdGuestResp = await guestClient.GetAsync($"/api/v1/recipes/{recipeId}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedByIdGuestResp.StatusCode);

        // 21. [2.15] Tác giả khôi phục bài viết từ thùng rác
        var restoreResponse = await client.PostAsync($"/api/v1/recipes/{recipeId}/restore", null);
        Assert.Equal(HttpStatusCode.NoContent, restoreResponse.StatusCode);

        var restoredDetailResponse = await client.GetAsync($"/api/v1/recipes/{recipeId}");
        var restoredDetail = await restoredDetailResponse.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(restoredDetail);
        Assert.Equal(RecipeStatus.Published, restoredDetail.Status);
    }

    private async Task<Guid> CreateSimpleRecipeAsync(HttpClient client, string title = "Công thức mẫu test")
    {
        var payload = new
        {
            Title = $"{title} {Guid.NewGuid():N}",
            Description = "Mô tả công thức mẫu phục vụ kiểm thử integration",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 30,
            Servings = 4,
            Difficulty = Difficulty.Easy
        };
        var resp = await client.PostAsJsonAsync("/api/v1/recipes", payload);
        resp.EnsureSuccessStatusCode();
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        return doc!.RootElement.GetProperty("id").GetGuid();
    }

    // ── Task 2.23: Kiểm thử mã lỗi 401 Unauthorized ──────────────────────────────
    [Fact]
    public async Task Endpoints_WithoutAuth_Return401Unauthorized()
    {
        using var authorClient = await CreateAuthorClientAsync();
        var recipeId = await CreateSimpleRecipeAsync(authorClient);

        using var guestClient = factory.CreateClient();

        var validUpdatePayload = new
        {
            Title = "Tiêu đề hợp lệ dài",
            Description = "Mô tả hợp lệ dài",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 20,
            Servings = 2,
            Difficulty = Difficulty.Easy,
            Version = 1u
        };

        // 1. /me/recipes yêu cầu xác thực
        var meResp = await guestClient.GetAsync("/api/v1/me/recipes");
        Assert.Equal(HttpStatusCode.Unauthorized, meResp.StatusCode);

        // 2. PUT /recipes/{id} yêu cầu xác thực
        var putResp = await guestClient.PutAsJsonAsync($"/api/v1/recipes/{recipeId}", validUpdatePayload);
        Assert.Equal(HttpStatusCode.Unauthorized, putResp.StatusCode);

        // 3. DELETE /recipes/{id} yêu cầu xác thực
        var deleteResp = await guestClient.DeleteAsync($"/api/v1/recipes/{recipeId}");
        Assert.Equal(HttpStatusCode.Unauthorized, deleteResp.StatusCode);

        // 4. POST /recipes/{id}/publish yêu cầu xác thực
        var publishResp = await guestClient.PostAsync($"/api/v1/recipes/{recipeId}/publish", null);
        Assert.Equal(HttpStatusCode.Unauthorized, publishResp.StatusCode);

        // 5. POST /recipes/{id}/steps yêu cầu xác thực
        var addStepResp = await guestClient.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new
        {
            Title = "B1",
            Description = "Mô tả bước hợp lệ",
            DurationMinutes = 10
        });
        Assert.Equal(HttpStatusCode.Unauthorized, addStepResp.StatusCode);

        // 6. PUT /recipes/{id}/steps/order yêu cầu xác thực
        var reorderResp = await guestClient.PutAsJsonAsync($"/api/v1/recipes/{recipeId}/steps/order", new { StepIds = new[] { Guid.NewGuid() } });
        Assert.Equal(HttpStatusCode.Unauthorized, reorderResp.StatusCode);

        // 7. POST /recipes/{id}/ingredients yêu cầu xác thực
        var addIngResp = await guestClient.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/ingredients", new
        {
            Name = "Muối",
            Quantity = (decimal?)10,
            Unit = "g"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, addIngResp.StatusCode);
    }

    // ── Task 2.23: Kiểm thử mã lỗi 403 Forbidden ────────────────────────────────
    [Fact]
    public async Task Stranger_ModifyingRecipeOrChildEntities_Returns403Forbidden()
    {
        using var authorClient = await CreateAuthorClientAsync();
        using var strangerClient = await CreateAuthorClientAsync();

        var recipeId = await CreateSimpleRecipeAsync(authorClient);

        // Tác giả thêm 1 bước và 1 nguyên liệu
        var addStepResp = await authorClient.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new
        {
            Title = "Bước ban đầu",
            Description = "Chi tiết bước thực hiện",
            DurationMinutes = 10
        });
        var stepDoc = await addStepResp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        var stepId = stepDoc!.RootElement.GetProperty("id").GetGuid();

        var addIngResp = await authorClient.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/ingredients", new
        {
            Name = "Đường cát",
            Quantity = (decimal?)100,
            Unit = "g"
        });
        var ingDoc = await addIngResp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        var ingId = ingDoc!.RootElement.GetProperty("id").GetGuid();

        // 1. Người lạ sửa bước -> 403
        var strangerUpdateStep = await strangerClient.PutAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/steps/{stepId}",
            new { Title = "Người lạ sửa bước", Description = "Mô tả sửa bước", DurationMinutes = 5 });
        Assert.Equal(HttpStatusCode.Forbidden, strangerUpdateStep.StatusCode);

        // 2. Người lạ xóa bước -> 403
        var strangerDeleteStep = await strangerClient.DeleteAsync($"/api/v1/recipes/{recipeId}/steps/{stepId}");
        Assert.Equal(HttpStatusCode.Forbidden, strangerDeleteStep.StatusCode);

        // 3. Người lạ sắp xếp lại bước -> 403
        var strangerReorder = await strangerClient.PutAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/steps/order",
            new { StepIds = new[] { stepId } });
        Assert.Equal(HttpStatusCode.Forbidden, strangerReorder.StatusCode);

        // 4. Người lạ sửa nguyên liệu -> 403
        var strangerUpdateIng = await strangerClient.PutAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/ingredients/{ingId}",
            new { Name = "Đường phèn", Quantity = (decimal?)50, Unit = "g" });
        Assert.Equal(HttpStatusCode.Forbidden, strangerUpdateIng.StatusCode);

        // 5. Người lạ xóa nguyên liệu -> 403
        var strangerDeleteIng = await strangerClient.DeleteAsync($"/api/v1/recipes/{recipeId}/ingredients/{ingId}");
        Assert.Equal(HttpStatusCode.Forbidden, strangerDeleteIng.StatusCode);

        // 6. Người lạ lưu trữ bài viết -> 403
        var strangerArchive = await strangerClient.PatchAsync($"/api/v1/recipes/{recipeId}/archive", null);
        Assert.Equal(HttpStatusCode.Forbidden, strangerArchive.StatusCode);

        // 7. Người lạ xóa bài viết -> 403
        var strangerDeleteRecipe = await strangerClient.DeleteAsync($"/api/v1/recipes/{recipeId}");
        Assert.Equal(HttpStatusCode.Forbidden, strangerDeleteRecipe.StatusCode);

        // Tác giả xóa bài viết vào thùng rác
        var authorDeleteRecipe = await authorClient.DeleteAsync($"/api/v1/recipes/{recipeId}");
        Assert.Equal(HttpStatusCode.NoContent, authorDeleteRecipe.StatusCode);

        // 8. Người lạ khôi phục bài viết -> 403
        var strangerRestore = await strangerClient.PostAsync($"/api/v1/recipes/{recipeId}/restore", null);
        Assert.Equal(HttpStatusCode.Forbidden, strangerRestore.StatusCode);
    }

    // ── Task 2.23: Kiểm thử mã lỗi 404 NotFound ─────────────────────────────────
    [Fact]
    public async Task OperationsOnNonExistentEntities_Return404NotFound()
    {
        using var client = await CreateAuthorClientAsync();
        var fakeId = Guid.NewGuid();

        var validUpdatePayload = new
        {
            Title = "Không tồn tại bài viết",
            Description = "Mô tả đầy đủ hợp lệ",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 20,
            Servings = 2,
            Difficulty = Difficulty.Easy,
            Version = 1u
        };

        // 1. GET /recipes/{id} với ID không tồn tại -> 404
        var getResp = await client.GetAsync($"/api/v1/recipes/{fakeId}");
        Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);

        // 2. PUT /recipes/{id} với ID không tồn tại -> 404
        var putResp = await client.PutAsJsonAsync($"/api/v1/recipes/{fakeId}", validUpdatePayload);
        Assert.Equal(HttpStatusCode.NotFound, putResp.StatusCode);

        // 3. DELETE /recipes/{id} với ID không tồn tại -> 404
        var deleteResp = await client.DeleteAsync($"/api/v1/recipes/{fakeId}");
        Assert.Equal(HttpStatusCode.NotFound, deleteResp.StatusCode);

        // 4. POST /recipes/{fakeId}/steps với Recipe ID không tồn tại -> 404
        var addStepResp = await client.PostAsJsonAsync($"/api/v1/recipes/{fakeId}/steps", new
        {
            Title = "B1",
            Description = "Mô tả hợp lệ",
            DurationMinutes = 10
        });
        Assert.Equal(HttpStatusCode.NotFound, addStepResp.StatusCode);

        // 5. POST /recipes/{fakeId}/ingredients với Recipe ID không tồn tại -> 404
        var addIngResp = await client.PostAsJsonAsync($"/api/v1/recipes/{fakeId}/ingredients", new
        {
            Name = "Muối",
            Quantity = (decimal?)10,
            Unit = "g"
        });
        Assert.Equal(HttpStatusCode.NotFound, addIngResp.StatusCode);

        // 6. PUT /recipes/{fakeId}/steps/order với Recipe ID không tồn tại -> 404
        var reorderResp = await client.PutAsJsonAsync($"/api/v1/recipes/{fakeId}/steps/order", new { StepIds = new[] { fakeId } });
        Assert.Equal(HttpStatusCode.NotFound, reorderResp.StatusCode);

        // Tạo 1 recipe hợp lệ để test thao tác trên step / ingredient giả
        var recipeId = await CreateSimpleRecipeAsync(client);
        var fakeChildId = Guid.NewGuid();

        // 7. PUT /recipes/{id}/steps/{stepId} với Step ID không tồn tại -> 404
        var updateFakeStepResp = await client.PutAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/steps/{fakeChildId}",
            new { Title = "Sửa bước giả", Description = "Mô tả bước hợp lệ", DurationMinutes = 5 });
        Assert.Equal(HttpStatusCode.NotFound, updateFakeStepResp.StatusCode);

        // 8. DELETE /recipes/{id}/steps/{stepId} với Step ID không tồn tại -> 404
        var deleteFakeStepResp = await client.DeleteAsync($"/api/v1/recipes/{recipeId}/steps/{fakeChildId}");
        Assert.Equal(HttpStatusCode.NotFound, deleteFakeStepResp.StatusCode);

        // 9. PUT /recipes/{id}/ingredients/{ingId} với Ingredient ID không tồn tại -> 404
        var updateFakeIngResp = await client.PutAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/ingredients/{fakeChildId}",
            new { Name = "Sửa gia vị giả", Quantity = (decimal?)10, Unit = "g" });
        Assert.Equal(HttpStatusCode.NotFound, updateFakeIngResp.StatusCode);

        // 10. DELETE /recipes/{id}/ingredients/{ingId} với Ingredient ID không tồn tại -> 404
        var deleteFakeIngResp = await client.DeleteAsync($"/api/v1/recipes/{recipeId}/ingredients/{fakeChildId}");
        Assert.Equal(HttpStatusCode.NotFound, deleteFakeIngResp.StatusCode);
    }

    // ── Task 2.23: Kiểm thử mã lỗi 409 Conflict (Concurrency) ───────────────────
    [Fact]
    public async Task UpdateRecipe_ParallelUpdatesWithSameVersion_CausesConcurrencyConflict409()
    {
        using var client = await CreateAuthorClientAsync();
        var recipeId = await CreateSimpleRecipeAsync(client, "Món bò xào hành tây");

        // Lấy version ban đầu
        var getResp = await client.GetAsync($"/api/v1/recipes/{recipeId}");
        var recipe = await getResp.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(recipe);
        var initialVersion = recipe.Version;

        // Lần cập nhật 1: Thành công với version ban đầu
        var update1 = new
        {
            Title = "Món bò xào hành tây cập nhật lần 1",
            Description = "Mô tả 1",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 20,
            Servings = 4,
            Difficulty = Difficulty.Medium,
            Version = initialVersion
        };
        var resp1 = await client.PutAsJsonAsync($"/api/v1/recipes/{recipeId}", update1);
        Assert.Equal(HttpStatusCode.NoContent, resp1.StatusCode);

        // Lần cập nhật 2: Sử dụng lại initialVersion cũ (stale version) -> 409 CONCURRENCY_CONFLICT
        var update2 = new
        {
            Title = "Món bò xào hành tây cập nhật lần 2",
            Description = "Mô tả 2",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 20,
            Servings = 4,
            Difficulty = Difficulty.Medium,
            Version = initialVersion
        };
        var resp2 = await client.PutAsJsonAsync($"/api/v1/recipes/{recipeId}", update2);
        Assert.Equal(HttpStatusCode.Conflict, resp2.StatusCode);

        var conflictDoc = await resp2.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.NotNull(conflictDoc);
        Assert.Equal("CONCURRENCY_CONFLICT", conflictDoc.RootElement.GetProperty("code").GetString());
    }

    // ── Task 2.23: Kiểm thử mã lỗi 422 UnprocessableEntity (Business Rules) ──────
    [Fact]
    public async Task Recipe_BusinessRuleViolations_Return422UnprocessableEntity()
    {
        using var client = await CreateAuthorClientAsync();

        // 1. Tạo recipe với CategoryId = Guid.Empty không hợp lệ -> 422 CATEGORY_INVALID
        var invalidCategoryPayload = new
        {
            Title = "Công thức với danh mục sai",
            Description = "Mô tả hợp lệ",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 20,
            Servings = 2,
            Difficulty = Difficulty.Easy,
            CategoryId = Guid.Empty
        };
        var invalidCatResp = await client.PostAsJsonAsync("/api/v1/recipes", invalidCategoryPayload);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidCatResp.StatusCode);

        // Tạo 1 recipe hợp lệ nhưng chưa có bước nào
        var recipeId = await CreateSimpleRecipeAsync(client);

        // 2. Xuất bản recipe có 0 bước -> 422 RECIPE_PUBLISH_INCOMPLETE
        var publishNoStepsResp = await client.PostAsync($"/api/v1/recipes/{recipeId}/publish", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, publishNoStepsResp.StatusCode);
        var publishNoStepsDoc = await publishNoStepsResp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.Equal(RecipeErrorCodes.PublishIncomplete, publishNoStepsDoc!.RootElement.GetProperty("code").GetString());

        // Chuyển bài viết sang Archived
        var archiveResp = await client.PatchAsync($"/api/v1/recipes/{recipeId}/archive", null);
        Assert.Equal(HttpStatusCode.NoContent, archiveResp.StatusCode);

        // 3. Xuất bản bài viết khi đang ở trạng thái Archived -> 422 INVALID_STATE_TRANSITION
        var publishArchivedResp = await client.PostAsync($"/api/v1/recipes/{recipeId}/publish", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, publishArchivedResp.StatusCode);
        var publishArchivedDoc = await publishArchivedResp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.Equal(RecipeErrorCodes.InvalidStateTransition, publishArchivedDoc!.RootElement.GetProperty("code").GetString());

        // 4. Hủy xuất bản bài viết khi đang ở trạng thái Archived -> 422 INVALID_STATE_TRANSITION
        var unpublishArchivedResp = await client.PostAsync($"/api/v1/recipes/{recipeId}/unpublish", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unpublishArchivedResp.StatusCode);
        var unpublishArchivedDoc = await unpublishArchivedResp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        Assert.Equal(RecipeErrorCodes.InvalidStateTransition, unpublishArchivedDoc!.RootElement.GetProperty("code").GetString());

        // Khôi phục về Draft để tiếp tục các kiểm thử bước tiếp theo
        var unarchiveResp = await client.PatchAsync($"/api/v1/recipes/{recipeId}/unarchive", null);
        Assert.Equal(HttpStatusCode.NoContent, unarchiveResp.StatusCode);

        // Thêm 2 bước để test sắp xếp sai thứ tự
        var step1Resp = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new
        {
            Title = "Bước 1",
            Description = "Mô tả bước 1",
            DurationMinutes = 5
        });
        var step1Doc = await step1Resp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        var step1Id = step1Doc!.RootElement.GetProperty("id").GetGuid();

        var step2Resp = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new
        {
            Title = "Bước 2",
            Description = "Mô tả bước 2",
            DurationMinutes = 5
        });
        var step2Doc = await step2Resp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        var step2Id = step2Doc!.RootElement.GetProperty("id").GetGuid();

        // 5. Sắp xếp lại bước với danh sách chứa ID trùng lặp -> 422 INVALID_STEP_ORDER
        var duplicateOrderResp = await client.PutAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/steps/order",
            new { StepIds = new[] { step1Id, step1Id } });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicateOrderResp.StatusCode);

        // 6. Sắp xếp lại bước với số lượng ID không khớp với recipe -> 422 INVALID_STEP_ORDER
        var mismatchedCountResp = await client.PutAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/steps/order",
            new { StepIds = new[] { step1Id } });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, mismatchedCountResp.StatusCode);
    }

    // ── Task 2.22 & 2.23: Integration test sắp xếp bước thực hiện (PUT /steps/order) ──
    [Fact]
    public async Task ReorderRecipeSteps_ValidOrder_ReordersStepsAndReturns204NoContent()
    {
        using var client = await CreateAuthorClientAsync();
        var recipeId = await CreateSimpleRecipeAsync(client, "Công thức kiểm thử sắp xếp bước");

        // Tạo 3 bước đầy đủ title và description
        var s1Resp = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new
        {
            Title = "Bước Sơ chế",
            Description = "Sơ chế nguyên liệu sạch sẽ",
            DurationMinutes = 10
        });
        var s1Doc = await s1Resp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        var s1Id = s1Doc!.RootElement.GetProperty("id").GetGuid();

        var s2Resp = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new
        {
            Title = "Bước Tẩm ướp",
            Description = "Tẩm ướp gia vị đậm đà",
            DurationMinutes = 20
        });
        var s2Doc = await s2Resp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        var s2Id = s2Doc!.RootElement.GetProperty("id").GetGuid();

        var s3Resp = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new
        {
            Title = "Bước Nấu nướng",
            Description = "Nấu chín đều thơm ngon",
            DurationMinutes = 30
        });
        var s3Doc = await s3Resp.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        var s3Id = s3Doc!.RootElement.GetProperty("id").GetGuid();

        // Đảo thứ tự: [Bước 3, Bước 1, Bước 2]
        var newOrder = new[] { s3Id, s1Id, s2Id };
        var reorderResp = await client.PutAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/steps/order",
            new { StepIds = newOrder });
        Assert.Equal(HttpStatusCode.NoContent, reorderResp.StatusCode);

        // Kiểm tra chi tiết bài viết: Thứ tự các bước phải cập nhật tương ứng
        var detailResp = await client.GetAsync($"/api/v1/recipes/{recipeId}");
        var detail = await detailResp.Content.ReadFromJsonAsync<RecipeDto>(JsonOptions);
        Assert.NotNull(detail);
        Assert.Equal(3, detail.Steps.Count);

        // Sắp xếp danh sách trả về theo StepNumber để assert
        var orderedSteps = detail.Steps.OrderBy(s => s.StepNumber).ToList();
        Assert.Equal(s3Id, orderedSteps[0].Id);
        Assert.Equal(1, orderedSteps[0].StepNumber);
        Assert.Equal("Bước Nấu nướng", orderedSteps[0].Title);

        Assert.Equal(s1Id, orderedSteps[1].Id);
        Assert.Equal(2, orderedSteps[1].StepNumber);
        Assert.Equal("Bước Sơ chế", orderedSteps[1].Title);

        Assert.Equal(s2Id, orderedSteps[2].Id);
        Assert.Equal(3, orderedSteps[2].StepNumber);
        Assert.Equal("Bước Tẩm ướp", orderedSteps[2].Title);
    }
}
