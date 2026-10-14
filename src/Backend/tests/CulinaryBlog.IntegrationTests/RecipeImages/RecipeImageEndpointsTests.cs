using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.IntegrationTests.Auth;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.IntegrationTests.RecipeImages;

[Collection(PostgresCollection.Name)]
public sealed class RecipeImageEndpointsTests(PostgresApiFactory factory)
{
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jR5kAAAAASUVORK5CYII=");

    [Fact]
    public async Task Owner_CanUploadChangePrimaryAndDeleteImage()
    {
        using var owner = CreateClient(Roles.Author);
        var recipeId = await CreateRecipeAsync(owner);

        using var firstUpload = await UploadAsync(owner, recipeId, Png, "image/png", "Ảnh đầu");
        Assert.True(firstUpload.StatusCode == HttpStatusCode.Created,
            await firstUpload.Content.ReadAsStringAsync());
        var first = await firstUpload.Content.ReadFromJsonAsync<ImageResponse>();
        Assert.NotNull(first);
        Assert.True(first.IsPrimary);
        Assert.EndsWith("/original.png", first.OriginalUrl);

        var storage = factory.Services.GetRequiredService<IFileStorage>();
        await using (var saved = await storage.OpenReadAsync($"recipes/{recipeId}/{first.ImageId}/original.png"))
        {
            using var copy = new MemoryStream();
            await saved.CopyToAsync(copy);
            Assert.Equal(Png, copy.ToArray());
        }

        using var secondUpload = await UploadAsync(owner, recipeId, Png, "image/png", null);
        Assert.Equal(HttpStatusCode.Created, secondUpload.StatusCode);
        var second = await secondUpload.Content.ReadFromJsonAsync<ImageResponse>();
        Assert.NotNull(second);
        Assert.False(second.IsPrimary);

        using var updated = await owner.PatchAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/images/{second.ImageId}",
            new { altText = "Ảnh chính mới", isPrimary = true, orderIndex = 2 });
        Assert.True(updated.StatusCode == HttpStatusCode.OK,
            await updated.Content.ReadAsStringAsync());

        var afterUpdate = await owner.GetFromJsonAsync<List<ImageResponse>>($"/api/v1/recipes/{recipeId}/images");
        Assert.NotNull(afterUpdate);
        Assert.Single(afterUpdate, image => image.IsPrimary);
        Assert.Equal(second.ImageId, afterUpdate.Single(image => image.IsPrimary).ImageId);
        Assert.Equal("Ảnh chính mới", afterUpdate.Single(image => image.ImageId == second.ImageId).AltText);

        using var deleted = await owner.DeleteAsync($"/api/v1/recipes/{recipeId}/images/{second.ImageId}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var remaining = await owner.GetFromJsonAsync<List<ImageResponse>>($"/api/v1/recipes/{recipeId}/images");
        Assert.NotNull(remaining);
        Assert.Single(remaining);
        Assert.Equal(first.ImageId, remaining[0].ImageId);
        Assert.True(remaining[0].IsPrimary);
    }

    [Fact]
    public async Task Upload_InvalidSignatureOrOversizedFile_ReturnsProblemDetailsWithoutImage()
    {
        using var owner = CreateClient(Roles.Author);
        var recipeId = await CreateRecipeAsync(owner);

        using var invalid = await UploadAsync(owner, recipeId, "not an image"u8.ToArray(), "image/png", null);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, invalid.StatusCode);
        Assert.Equal("FILE_TYPE_NOT_ALLOWED", await AuthApi.ReadErrorCodeAsync(invalid));

        var oversized = new byte[5 * 1024 * 1024 + 1];
        Png.CopyTo(oversized, 0);
        using var tooLarge = await UploadAsync(owner, recipeId, oversized, "image/png", null);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, tooLarge.StatusCode);
        Assert.Equal("FILE_SIZE_EXCEEDED", await AuthApi.ReadErrorCodeAsync(tooLarge));

        var images = await owner.GetFromJsonAsync<List<ImageResponse>>($"/api/v1/recipes/{recipeId}/images");
        Assert.NotNull(images);
        Assert.Empty(images);
    }

    [Fact]
    public async Task OtherAuthor_IsForbidden_ButAdminCanUpload()
    {
        using var owner = CreateClient(Roles.Author);
        using var otherAuthor = CreateClient(Roles.Author);
        using var admin = CreateClient(Roles.Admin);
        var recipeId = await CreateRecipeAsync(owner);

        using var forbidden = await UploadAsync(otherAuthor, recipeId, Png, "image/png", null);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using var allowed = await UploadAsync(admin, recipeId, Png, "image/png", null);
        Assert.Equal(HttpStatusCode.Created, allowed.StatusCode);
    }

    private HttpClient CreateClient(string role)
    {
        var client = factory.CreateClient();
        var issuer = factory.Services.GetRequiredService<IAccessTokenIssuer>();
        var user = new UserAccount(Guid.NewGuid(), "image-test@example.com", "image-test", "Image Test",
            null, true, DateTimeOffset.UtcNow, [role]);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", issuer.Issue(user).Value);
        return client;
    }

    private static async Task<Guid> CreateRecipeAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/recipes", new
        {
            title = $"Công thức ảnh {Guid.NewGuid():N}",
            description = "Kiểm thử ảnh công thức",
            prepTimeMinutes = 10,
            cookTimeMinutes = 10,
            servings = 2,
            difficulty = 0,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatedRecipe>();
        Assert.NotNull(created);
        return created.Id;
    }

    private static async Task<HttpResponseMessage> UploadAsync(
        HttpClient client, Guid recipeId, byte[] bytes, string contentType, string? altText)
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", "photo.png");
        if (altText is not null) form.Add(new StringContent(altText), "altText");
        return await client.PostAsync($"/api/v1/recipes/{recipeId}/images", form);
    }

    private sealed record CreatedRecipe(Guid Id);
    private sealed record ImageResponse(Guid ImageId, string OriginalUrl, string? AltText, bool IsPrimary);
}
