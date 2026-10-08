using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

public sealed record GetCategoryBySlugQuery(string Slug, int Page = 1, int PageSize = PagedResult<object>.DefaultPageSize)
    : IRequest<CategoryDetailDto>;

public sealed class GetCategoryBySlugQueryValidator : AbstractValidator<GetCategoryBySlugQuery>
{
    public GetCategoryBySlugQueryValidator()
    {
        RuleFor(query => query.Slug).NotEmpty().MaximumLength(120);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PagedResult<object>.MaxPageSize);
    }
}

public sealed class GetCategoryBySlugQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetCategoryBySlugQuery, CategoryDetailDto>
{
    public Task<CategoryDetailDto> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var category = db.Categories
            .Where(item => item.Slug == request.Slug)
            .Select(item => new CategoryDto(
                item.Id,
                item.Name,
                item.Slug,
                item.Description,
                item.ImageUrl,
                item.OrderIndex,
                db.Recipes.Count(recipe =>
                    recipe.CategoryId == item.Id && recipe.Status == RecipeStatus.Published)))
            .FirstOrDefault();

        if (category is null)
        {
            throw new NotFoundException(
                $"Không tìm thấy danh mục có slug '{request.Slug}'.",
                CategoryErrorCodes.CategoryNotFound);
        }

        var ownerId = currentUser.IsInRole(Roles.Author) ? currentUser.UserId?.ToString() : null;
        var recipes = db.Recipes
            .Where(recipe => recipe.CategoryId == category.Id &&
                (recipe.Status == RecipeStatus.Published ||
                 (ownerId != null && recipe.Status == RecipeStatus.Draft && recipe.AuthorId == ownerId)))
            .OrderByDescending(recipe => recipe.PublishedAt)
            .ThenByDescending(recipe => recipe.CreatedAt);

        var totalCount = recipes.Count();
        var offset = ((long)request.Page - 1) * request.PageSize;
        IReadOnlyList<CategoryRecipeSummaryDto> items = offset > int.MaxValue
            ? []
            : recipes
                .Skip((int)offset)
                .Take(request.PageSize)
                .Select(recipe => new CategoryRecipeSummaryDto(
                    recipe.Id,
                    recipe.Title,
                    recipe.Slug,
                    recipe.Description,
                    recipe.PrepTimeMinutes,
                    recipe.CookTimeMinutes,
                    recipe.Servings,
                    recipe.Difficulty,
                    recipe.PublishedAt))
                .ToList();

        return Task.FromResult(new CategoryDetailDto(
            category,
            new PagedResult<CategoryRecipeSummaryDto>(items, request.Page, request.PageSize, totalCount)));
    }
}

public sealed record CategoryDetailDto(
    CategoryDto Category,
    PagedResult<CategoryRecipeSummaryDto> Recipes);

public sealed record CategoryRecipeSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    Difficulty Difficulty,
    DateTimeOffset? PublishedAt);

public static class CategoryErrorCodes
{
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
}
