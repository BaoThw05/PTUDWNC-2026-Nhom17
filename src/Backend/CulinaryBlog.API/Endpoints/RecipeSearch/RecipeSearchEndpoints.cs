using CulinaryBlog.Application.Features.RecipeSearch;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.API.Endpoints.RecipeSearch;

internal sealed class RecipeSearchEndpoints : IEndpointModule
{
    public string Tag => "RecipeSearch";

    public string Description => "Danh sách công thức công khai và tìm kiếm (TV4)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var recipeSearch = api.MapGroup("/recipes").WithTags(Tag);

        recipeSearch.MapGet("/", async (
            Guid? categoryId,
            string? difficulty,
            int? maxCookTime,
            int? minServings,
            string? sort,
            int? page,
            int? pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetRecipesQuery(
                categoryId,
                difficulty,
                maxCookTime,
                minServings,
                sort ?? "-createdAt",
                page ?? 1,
                pageSize ?? 12);

            return Results.Ok(await sender.Send(query, cancellationToken));
        })
        .WithName("ListRecipes")
        .WithSummary("List recipes visible to the current caller")
        .CacheOutput("RecipeList")
        .ProducesValidationProblem();

        recipeSearch.MapGet("/search", async (
            string? q,
            Guid? categoryId,
            string? difficulty,
            int? maxCookTime,
            int? minServings,
            string? sort,
            int? page,
            int? pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchRecipesQuery(
                q ?? string.Empty,
                categoryId,
                difficulty,
                maxCookTime,
                minServings,
                sort,
                page ?? 1,
                pageSize ?? 10);

            return Results.Ok(await sender.Send(query, cancellationToken));
        })
        .WithName("SearchRecipes")
        .WithSummary("Search published recipes using Vietnamese full-text and trigram matching")
        .ProducesValidationProblem();
    }
}
