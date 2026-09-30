using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.RecipeSearch;

public sealed record GetRecipesQuery(
    Guid? CategoryId = null,
    string? Difficulty = null,
    int? MaxCookTime = null,
    int? MinServings = null,
    string Sort = "-createdAt",
    int Page = 1,
    int PageSize = PagedResult<RecipeSummaryDto>.DefaultPageSize)
    : IRequest<PagedResult<RecipeSummaryDto>>;

public sealed class GetRecipesQueryValidator : AbstractValidator<GetRecipesQuery>
{
    private static readonly string[] SortFields = ["createdAt", "title", "cookTime"];

    public GetRecipesQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PagedResult<RecipeSummaryDto>.MaxPageSize);
        RuleFor(query => query.MaxCookTime).GreaterThanOrEqualTo(0).When(query => query.MaxCookTime.HasValue);
        RuleFor(query => query.MinServings).GreaterThan(0).When(query => query.MinServings.HasValue);
        RuleFor(query => query.Difficulty)
            .Must(value => string.IsNullOrWhiteSpace(value) ||
                           (Enum.TryParse<Difficulty>(value, true, out var difficulty) && Enum.IsDefined(difficulty)))
            .WithMessage("Difficulty must be Easy, Medium, or Hard.");
        RuleFor(query => query.Sort)
            .Must(IsAllowedSort)
            .WithMessage("Sort must be createdAt, title, cookTime, or one of those fields prefixed with '-'.");
    }

    private static bool IsAllowedSort(string sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return false;
        }

        var field = sort.StartsWith("-", StringComparison.Ordinal) ? sort[1..] : sort;
        return SortFields.Contains(field, StringComparer.OrdinalIgnoreCase);
    }
}

public sealed class GetRecipesQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public Task<PagedResult<RecipeSummaryDto>> Handle(
        GetRecipesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Recipe> query = db.Recipes;
        var callerId = currentUser.UserId?.ToString();

        if (!currentUser.IsInRole(Roles.Admin))
        {
            if (string.IsNullOrWhiteSpace(callerId))
            {
                query = query.Where(recipe => recipe.Status == RecipeStatus.Published);
            }
            else
            {
                query = query.Where(recipe =>
                    recipe.Status == RecipeStatus.Published ||
                    (recipe.AuthorId == callerId &&
                     (recipe.Status == RecipeStatus.Draft || recipe.Status == RecipeStatus.Archived)));
            }
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(recipe => recipe.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Difficulty) &&
            Enum.TryParse<Difficulty>(request.Difficulty, true, out var difficulty))
        {
            query = query.Where(recipe => recipe.Difficulty == difficulty);
        }

        if (request.MaxCookTime.HasValue)
        {
            query = query.Where(recipe => recipe.CookTimeMinutes <= request.MaxCookTime.Value);
        }

        if (request.MinServings.HasValue)
        {
            query = query.Where(recipe => recipe.Servings >= request.MinServings.Value);
        }

        var totalCount = query.Count();
        var descending = request.Sort.StartsWith("-", StringComparison.Ordinal);
        var field = descending ? request.Sort[1..] : request.Sort;

        IOrderedQueryable<Recipe> orderedQuery = (field.ToLowerInvariant(), descending) switch
        {
            ("createdat", true) => query.OrderByDescending(recipe => recipe.CreatedAt),
            ("createdat", false) => query.OrderBy(recipe => recipe.CreatedAt),
            ("title", true) => query.OrderByDescending(recipe => recipe.Title),
            ("title", false) => query.OrderBy(recipe => recipe.Title),
            ("cooktime", true) => query.OrderByDescending(recipe => recipe.CookTimeMinutes),
            ("cooktime", false) => query.OrderBy(recipe => recipe.CookTimeMinutes),
            _ => query.OrderByDescending(recipe => recipe.CreatedAt)
        };

        var items = orderedQuery
            .ThenBy(recipe => recipe.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(recipe => new RecipeSummaryDto(
                recipe.Id,
                recipe.Title,
                recipe.Slug,
                recipe.Description,
                recipe.PrepTimeMinutes,
                recipe.CookTimeMinutes,
                recipe.Servings,
                recipe.Difficulty,
                recipe.Status,
                recipe.PublishedAt,
                recipe.CategoryId,
                null))
            .ToList();

        return Task.FromResult(new PagedResult<RecipeSummaryDto>(items, request.Page, request.PageSize, totalCount));
    }
}
