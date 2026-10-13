using CulinaryBlog.API.Auth;
using CulinaryBlog.Application.Features.Categories;
using MediatR;

namespace CulinaryBlog.API.Endpoints.Categories;

internal sealed class CategoriesEndpoints : IEndpointModule
{
    public string Tag => "Categories";

    public string Description => "Danh mục công thức (TV3)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var categories = api.MapGroup("/categories").WithTags(Tag);

        categories.MapPost("/", async (
            CreateCategoryCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/v1/categories/{result.Slug}", result);
        })
        .WithName("CreateCategory")
        .RequireAuthorization(AuthPolicies.Admin);

        categories.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCategoryCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command with { Id = id }, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpdateCategory")
        .RequireAuthorization(AuthPolicies.Admin);

        categories.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteCategory")
        .RequireAuthorization(AuthPolicies.Admin);
    }
}
