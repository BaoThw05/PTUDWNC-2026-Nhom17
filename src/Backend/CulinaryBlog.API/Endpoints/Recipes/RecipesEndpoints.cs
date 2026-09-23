using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.API.Endpoints.Recipes;

internal sealed class RecipesEndpoints : IEndpointModule
{
    public string Tag => "Recipes";

    public string Description => "Tạo, sửa, publish, archive, thùng rác công thức; bước và nguyên liệu (TV2)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var recipes = api.MapGroup("/recipes").WithTags(Tag);

        recipes.MapPost("/", async (CreateRecipeCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Created($"/api/v1/recipes/{id}", new { id });
        })
        .WithName("CreateRecipe");

        recipes.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var recipe = await mediator.Send(new GetRecipeByIdQuery(id));
            return Results.Ok(recipe);
        })
        .WithName("GetRecipeById");

        recipes.MapGet("/{slug}", async (string slug, IMediator mediator) =>
        {
            var recipe = await mediator.Send(new GetRecipeBySlugQuery(slug));
            return Results.Ok(recipe);
        })
        .WithName("GetRecipeBySlug");

        recipes.MapPut("/{id:guid}", async (Guid id, UpdateRecipeCommand command, IMediator mediator) =>
        {
            await mediator.Send(command with { Id = id });
            return Results.NoContent();
        })
        .WithName("UpdateRecipe");

        recipes.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new DeleteRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("DeleteRecipe");

        recipes.MapPost("/{id:guid}/publish", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new PublishRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("PublishRecipe");

        recipes.MapPost("/{id:guid}/unpublish", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new UnpublishRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("UnpublishRecipe");

        recipes.MapPost("/{id:guid}/archive", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new ArchiveRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("ArchiveRecipe");

        recipes.MapPost("/{id:guid}/unarchive", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new UnarchiveRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("UnarchiveRecipe");

        recipes.MapPost("/{id:guid}/restore", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new RestoreRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("RestoreRecipe");

        // Steps (FR-RCP-010)
        recipes.MapPost("/{id:guid}/steps", async (Guid id, AddRecipeStepCommand command, IMediator mediator) =>
        {
            var stepId = await mediator.Send(command with { RecipeId = id });
            return Results.Created($"/api/v1/recipes/{id}/steps/{stepId}", new { id = stepId });
        })
        .WithName("AddRecipeStep");

        recipes.MapDelete("/{id:guid}/steps/{stepId:guid}", async (Guid id, Guid stepId, IMediator mediator) =>
        {
            await mediator.Send(new DeleteRecipeStepCommand(id, stepId));
            return Results.NoContent();
        })
        .WithName("DeleteRecipeStep");

        // Ingredients (FR-RCP-009)
        recipes.MapPost("/{id:guid}/ingredients", async (Guid id, AddRecipeIngredientCommand command, IMediator mediator) =>
        {
            var ingredientId = await mediator.Send(command with { RecipeId = id });
            return Results.Created($"/api/v1/recipes/{id}/ingredients/{ingredientId}", new { id = ingredientId });
        })
        .WithName("AddRecipeIngredient");

        recipes.MapDelete("/{id:guid}/ingredients/{ingredientId:guid}", async (Guid id, Guid ingredientId, IMediator mediator) =>
        {
            await mediator.Send(new DeleteRecipeIngredientCommand(id, ingredientId));
            return Results.NoContent();
        })
        .WithName("DeleteRecipeIngredient");

        // Bài của tôi (FR-RCP-003, S-11)
        api.MapGet("/me/recipes", (RecipeStatus? status, int? page, int? pageSize) =>
        {
            var result = new PagedResult<RecipeDto>(
                Array.Empty<RecipeDto>(),
                page ?? 1,
                pageSize ?? PagedResult<RecipeDto>.DefaultPageSize,
                0);
            return Results.Ok(result);
        })
        .WithTags(Tag)
        .WithName("GetMyRecipes");
    }
}
