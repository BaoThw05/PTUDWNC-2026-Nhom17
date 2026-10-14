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
        .WithName("CreateRecipe")
        .WithSummary("Tạo công thức mới dạng bản nháp (FR-RCP-002)");

        recipes.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var recipe = await mediator.Send(new GetRecipeByIdQuery(id));
            return Results.Ok(recipe);
        })
        .WithName("GetRecipeById")
        .WithSummary("Xem chi tiết công thức theo ID (FR-RCP-004)");

        recipes.MapGet("/{slug}", async (string slug, IMediator mediator) =>
        {
            var recipe = await mediator.Send(new GetRecipeBySlugQuery(slug));
            return Results.Ok(recipe);
        })
        .WithName("GetRecipeBySlug")
        .WithSummary("Xem chi tiết công thức công khai theo slug (FR-RCP-005)");

        recipes.MapPut("/{id:guid}", async (Guid id, UpdateRecipeCommand command, IMediator mediator) =>
        {
            await mediator.Send(command with { Id = id });
            return Results.NoContent();
        })
        .WithName("UpdateRecipe")
        .WithSummary("Cập nhật công thức nấu ăn (FR-RCP-003)");

        recipes.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new DeleteRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("DeleteRecipe")
        .WithSummary("Xóa công thức vào thùng rác (FR-RCP-007)");

        recipes.MapPost("/{id:guid}/publish", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new PublishRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("PublishRecipe")
        .WithSummary("Xuất bản công thức (FR-RCP-006)");

        recipes.MapPatch("/{id:guid}/publish", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new PublishRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("PublishRecipePatch")
        .WithSummary("Xuất bản công thức - Patch (FR-RCP-006)");

        recipes.MapPost("/{id:guid}/unpublish", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new UnpublishRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("UnpublishRecipe")
        .WithSummary("Hủy xuất bản công thức về bản nháp (FR-RCP-006)");

        recipes.MapPatch("/{id:guid}/unpublish", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new UnpublishRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("UnpublishRecipePatch")
        .WithSummary("Hủy xuất bản công thức về bản nháp - Patch (FR-RCP-006)");

        recipes.MapPost("/{id:guid}/archive", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new ArchiveRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("ArchiveRecipe")
        .WithSummary("Lưu trữ công thức (FR-RCP-006)");

        recipes.MapPatch("/{id:guid}/archive", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new ArchiveRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("ArchiveRecipePatch")
        .WithSummary("Lưu trữ công thức - Patch (FR-RCP-006)");

        recipes.MapPost("/{id:guid}/unarchive", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new UnarchiveRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("UnarchiveRecipe")
        .WithSummary("Bỏ lưu trữ công thức về bản nháp (FR-RCP-006)");

        recipes.MapPatch("/{id:guid}/unarchive", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new UnarchiveRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("UnarchiveRecipePatch")
        .WithSummary("Bỏ lưu trữ công thức về bản nháp - Patch (FR-RCP-006)");

        recipes.MapPost("/{id:guid}/restore", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new RestoreRecipeCommand(id));
            return Results.NoContent();
        })
        .WithName("RestoreRecipe")
        .WithSummary("Khôi phục công thức từ thùng rác (FR-RCP-007)");

        // Steps (FR-RCP-010)
        recipes.MapPost("/{id:guid}/steps", async (Guid id, AddRecipeStepCommand command, IMediator mediator) =>
        {
            var stepId = await mediator.Send(command with { RecipeId = id });
            return Results.Created($"/api/v1/recipes/{id}/steps/{stepId}", new { id = stepId });
        })
        .WithName("AddRecipeStep")
        .WithSummary("Thêm bước thực hiện mới (FR-RCP-010)");

        recipes.MapPut("/{id:guid}/steps/{stepId:guid}", async (Guid id, Guid stepId, UpdateRecipeStepCommand command, IMediator mediator) =>
        {
            await mediator.Send(command with { RecipeId = id, StepId = stepId });
            return Results.NoContent();
        })
        .WithName("UpdateRecipeStep")
        .WithSummary("Cập nhật bước thực hiện (FR-RCP-010)");

        recipes.MapDelete("/{id:guid}/steps/{stepId:guid}", async (Guid id, Guid stepId, IMediator mediator) =>
        {
            await mediator.Send(new DeleteRecipeStepCommand(id, stepId));
            return Results.NoContent();
        })
        .WithName("DeleteRecipeStep")
        .WithSummary("Xóa bước thực hiện (FR-RCP-010)");

        // Ingredients (FR-RCP-009)
        recipes.MapPost("/{id:guid}/ingredients", async (Guid id, AddRecipeIngredientCommand command, IMediator mediator) =>
        {
            var ingredientId = await mediator.Send(command with { RecipeId = id });
            return Results.Created($"/api/v1/recipes/{id}/ingredients/{ingredientId}", new { id = ingredientId });
        })
        .WithName("AddRecipeIngredient")
        .WithSummary("Thêm nguyên liệu mới (FR-RCP-009)");

        recipes.MapPut("/{id:guid}/ingredients/{ingredientId:guid}", async (Guid id, Guid ingredientId, UpdateRecipeIngredientCommand command, IMediator mediator) =>
        {
            await mediator.Send(command with { RecipeId = id, IngredientId = ingredientId });
            return Results.NoContent();
        })
        .WithName("UpdateRecipeIngredient")
        .WithSummary("Cập nhật nguyên liệu (FR-RCP-009)");

        recipes.MapDelete("/{id:guid}/ingredients/{ingredientId:guid}", async (Guid id, Guid ingredientId, IMediator mediator) =>
        {
            await mediator.Send(new DeleteRecipeIngredientCommand(id, ingredientId));
            return Results.NoContent();
        })
        .WithName("DeleteRecipeIngredient")
        .WithSummary("Xóa nguyên liệu (FR-RCP-009)");

        // Bài của tôi (FR-RCP-003, S-11)
        api.MapGet("/me/recipes", async (string? status, int? page, int? pageSize, IMediator mediator) =>
        {
            var query = new GetMyRecipesQuery(status, page ?? 1, pageSize ?? PagedResult<RecipeDto>.DefaultPageSize);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithTags(Tag)
        .WithName("GetMyRecipes")
        .WithSummary("Xem danh sách công thức của tôi (FR-RCP-003)");
    }
}
