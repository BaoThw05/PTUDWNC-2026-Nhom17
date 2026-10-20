using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Categories;
using MediatR;

namespace CulinaryBlog.API.Endpoints.Categories;

internal sealed class CategoryReadEndpoints : IEndpointModule
{
    public string Tag => "Categories";

    public string Description => "API đọc danh mục và công thức theo danh mục (TV3)";

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
    }
}
