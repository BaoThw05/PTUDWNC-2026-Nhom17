using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.RecipeSearch;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.IntegrationTests.Auth;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Storage;

namespace CulinaryBlog.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class RecipeSearchEndpointsTests(PostgresApiFactory factory)
{
    [Fact]
    public async Task PostgreSqlFixtureAppliesSearchMigration()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
        Assert.Contains("20260929163617_RecipeVietnameseSearch", appliedMigrations);
    }

    [Fact]
    public async Task RecipeSeederCreatesFiftyRecipesForFiveAuthorsAndIsIdempotent()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sampleRecipes = db.Recipes.IgnoreQueryFilters().Where(recipe => recipe.Slug.StartsWith("sample-"));

        Assert.Equal(50, await sampleRecipes.CountAsync());
        Assert.Equal(5, await sampleRecipes.Select(recipe => recipe.AuthorId).Distinct().CountAsync());
        Assert.Equal(
            await sampleRecipes.CountAsync(recipe => recipe.Status == RecipeStatus.Published),
            await db.RecipeSteps.CountAsync(step =>
                step.Recipe.Slug.StartsWith("sample-") && step.Recipe.Status == RecipeStatus.Published));

        var recipeSeeder = scope.ServiceProvider.GetServices<IDataSeeder>()
            .Single(seeder => seeder.GetType().Name == "RecipeDataSeeder");
        await recipeSeeder.SeedAsync(CancellationToken.None);

        Assert.Equal(50, await sampleRecipes.CountAsync());
    }

    [Fact]
    public async Task SearchIndexes_AreUsedByPostgreSqlPlans()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("SET LOCAL enable_seqscan = off");
        var connection = db.Database.GetDbConnection();

        var fullTextPlan = await ReadExplainPlanAsync(
            connection,
            transaction.GetDbTransaction(),
            "EXPLAIN SELECT \"Id\" FROM \"Recipes\" WHERE \"SearchVector\" @@ to_tsquery('public.vietnamese', 'pho:* & bo:*')");
        var trigramPlan = await ReadExplainPlanAsync(
            connection,
            transaction.GetDbTransaction(),
            "EXPLAIN SELECT \"Id\" FROM \"Recipes\" WHERE \"SearchText\" % 'pho bo'");

        Assert.Contains("IX_Recipes_SearchVector", fullTextPlan);
        Assert.Contains("IX_Recipes_SearchText_Trgm", trigramPlan);
    }

    [Fact]
    public async Task ListRecipes_GuestSeesOnlyPublishedAndPaginationMetadata()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/recipes?page=1&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<RecipeSummaryDto>>();
        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(5, result.Items.Count);
        Assert.True(result.TotalCount >= 35);
        Assert.All(result.Items, recipe => Assert.Equal(RecipeStatus.Published, recipe.Status));
    }

    [Fact]
    public async Task ListRecipes_AuthorSeesOwnDraft_AndGuestDoesNot()
    {
        using var authorClient = await CreateAuthorClientAsync();
        var createResponse = await authorClient.PostAsJsonAsync("/api/v1/recipes", new
        {
            Title = "Bánh cuốn bản nháp cá nhân",
            Description = "Công thức bản nháp chỉ tác giả sở hữu mới xem được.",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 20,
            Servings = 2,
            Difficulty = Difficulty.Easy
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(created);
        var draftId = created.RootElement.GetProperty("id").GetGuid();

        var authorList = await authorClient.GetAsync("/api/v1/recipes?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, authorList.StatusCode);
        var authorResult = await authorList.Content.ReadFromJsonAsync<PagedResult<RecipeSummaryDto>>();
        Assert.NotNull(authorResult);
        Assert.Contains(authorResult.Items, recipe => recipe.Id == draftId && recipe.Status == RecipeStatus.Draft);

        using var guestClient = factory.CreateClient();
        var guestList = await guestClient.GetAsync("/api/v1/recipes?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, guestList.StatusCode);
        var guestResult = await guestList.Content.ReadFromJsonAsync<PagedResult<RecipeSummaryDto>>();
        Assert.NotNull(guestResult);
        Assert.DoesNotContain(guestResult.Items, recipe => recipe.Id == draftId);
    }

    [Fact]
    public async Task ListRecipes_OwnerSeesArchivedAndAdminSeesAllStatuses()
    {
        using var ownerClient = await CreateSignedInClientAsync("author5@culinaryblog.test", AuthApi.StrongPassword);
        var ownerResponse = await ownerClient.GetAsync("/api/v1/recipes?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
        var ownerResult = await ownerResponse.Content.ReadFromJsonAsync<PagedResult<RecipeSummaryDto>>();
        Assert.NotNull(ownerResult);
        Assert.Contains(ownerResult.Items, recipe => recipe.Status == RecipeStatus.Archived);

        using var adminClient = await CreateSignedInClientAsync(
            PostgresApiFactory.AdminEmail,
            PostgresApiFactory.AdminPassword);
        var adminResponse = await adminClient.GetAsync("/api/v1/recipes?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        var adminResult = await adminResponse.Content.ReadFromJsonAsync<PagedResult<RecipeSummaryDto>>();
        Assert.NotNull(adminResult);
        Assert.True(adminResult.TotalCount >= 50);
        Assert.Contains(adminResult.Items, recipe => recipe.Status == RecipeStatus.Draft);
        Assert.Contains(adminResult.Items, recipe => recipe.Status == RecipeStatus.Archived);
    }

    [Fact]
    public async Task ListRecipes_PageSizeAboveMaximumReturns422()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/recipes?pageSize=51");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", await AuthApi.ReadErrorCodeAsync(response));
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Expert")]
    public async Task ListRecipes_InvalidDifficultyReturns422(string difficulty)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/recipes?difficulty={difficulty}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", await AuthApi.ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task ListRecipes_UnknownCategoryReturnsEmptyPage()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/recipes?categoryId={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<RecipeSummaryDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Theory]
    [InlineData("phở bò")]
    [InlineData("pho bo")]
    [InlineData("Pho Bo")]
    [InlineData("phơ bo")]
    [InlineData("bò")]
    public async Task SearchRecipes_MatchesVietnameseAccentVariants(string searchTerm)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/recipes/search?q={Uri.EscapeDataString(searchTerm)}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<RecipeSummaryDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.Contains(result.Items, recipe => recipe.Title.StartsWith("Phở bò", StringComparison.OrdinalIgnoreCase));
        Assert.All(result.Items, recipe => Assert.NotNull(recipe.RelevanceScore));
    }

    [Fact]
    public async Task SearchRecipes_OneCharacterReturns422()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/recipes/search?q=a");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", await AuthApi.ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task SearchRecipes_MissingTermReturns422()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/recipes/search");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", await AuthApi.ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task SearchRecipes_PunctuationOnlyTermReturnsEmptyPageWithoutSqlError()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/recipes/search?q=%25%25");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<RecipeSummaryDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Theory]
    [InlineData("%25%25")]
    [InlineData("%27%25")]
    public async Task SearchRecipes_SpecialCharactersAreTreatedAsInput(string encodedSearchTerm)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/recipes/search?q={encodedSearchTerm}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<RecipeSummaryDto>>();
        Assert.NotNull(result);
    }

    private async Task<HttpClient> CreateAuthorClientAsync()
    {
        var client = factory.CreateClient();
        var auth = await AuthApi.ReadAuthAsync(await AuthApi.RegisterAsync(client, AuthApi.NewEmail()));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }

    private async Task<HttpClient> CreateSignedInClientAsync(string email, string password)
    {
        var client = factory.CreateClient();
        var auth = await AuthApi.ReadAuthAsync(await AuthApi.LoginAsync(client, email, password));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }

    private static async Task<string> ReadExplainPlanAsync(
        System.Data.Common.DbConnection connection,
        System.Data.Common.DbTransaction transaction,
        string sql)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();
        var planLines = new List<string>();
        while (await reader.ReadAsync())
        {
            planLines.Add(reader.GetString(0));
        }

        return string.Join(Environment.NewLine, planLines);
    }
}
