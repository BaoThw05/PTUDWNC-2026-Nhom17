using CulinaryBlog.Application.Common.Models;
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

        categories.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetCategoriesQuery(), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetCategories");

        categories.MapGet("/{slug}", async (
            string slug,
            int? page,
            int? pageSize,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new GetCategoryBySlugQuery(
                    slug,
                    page ?? 1,
                    pageSize ?? PagedResult<object>.DefaultPageSize),
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetCategoryBySlug");

        // TODO(TV1): thêm RequireAuthorization(policy => policy.RequireRole("Admin")) khi JWT/policy đã được merge.
        categories.MapPost("/", async (
            CreateCategoryCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/v1/categories/{result.Id}", result);
        })
        .WithName("CreateCategory");

        categories.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCategoryCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command with { Id = id }, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpdateCategory");

        categories.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteCategory");
    }
}
