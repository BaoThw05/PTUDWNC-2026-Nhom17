using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

public sealed class GetCategoriesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<CategoryDto> categories = db.Categories
            .OrderBy(category => category.OrderIndex)
            .ThenBy(category => category.Name)
            .Select(category => new CategoryDto(
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.ImageUrl,
                db.Recipes.Count(recipe =>
                    recipe.CategoryId == category.Id && recipe.Status == RecipeStatus.Published)))
            .ToList();

        return Task.FromResult(categories);
    }
}

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int RecipeCount);
