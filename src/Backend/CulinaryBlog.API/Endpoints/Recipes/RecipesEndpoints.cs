using CulinaryBlog.Application.Features.Recipes;
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

        recipes.MapPost("/{id:guid}/restore", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new RestoreRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("RestoreRecipe");
    }
}
