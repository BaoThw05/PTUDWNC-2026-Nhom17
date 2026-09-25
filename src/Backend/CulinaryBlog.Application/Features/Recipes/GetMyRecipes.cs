using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record GetMyRecipesQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = PagedResult<RecipeDto>.DefaultPageSize) : IRequest<PagedResult<RecipeDto>>;

public sealed class GetMyRecipesQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetMyRecipesQuery, PagedResult<RecipeDto>>
{
    public Task<PagedResult<RecipeDto>> Handle(GetMyRecipesQuery request, CancellationToken cancellationToken)
    {
        var authorId = currentUser.UserId?.ToString();
        if (string.IsNullOrWhiteSpace(authorId))
        {
            throw new UnauthorizedException("Bạn cần đăng nhập để xem danh sách bài viết của mình.");
        }

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize switch
        {
            < 1 => PagedResult<RecipeDto>.DefaultPageSize,
            > PagedResult<RecipeDto>.MaxPageSize => PagedResult<RecipeDto>.MaxPageSize,
            _ => request.PageSize
        };

        IQueryable<Recipe> query;

        // Nếu lọc thùng rác (S-03 / 2.09)
        if (string.Equals(request.Status, "deleted", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(request.Status, "trash", StringComparison.OrdinalIgnoreCase))
        {
            query = db.RecipesIncludingDeleted
                .Where(r => r.AuthorId == authorId && r.IsDeleted);
        }
        else
        {
            query = db.Recipes.Where(r => r.AuthorId == authorId);

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<RecipeStatus>(request.Status, ignoreCase: true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }
        }

        var totalCount = query.Count();

        var items = query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RecipeDto
            {
                Id = r.Id,
                Title = r.Title,
                Slug = r.Slug,
                Description = r.Description,
                PrepTimeMinutes = r.PrepTimeMinutes,
                CookTimeMinutes = r.CookTimeMinutes,
                Servings = r.Servings,
                Difficulty = r.Difficulty,
                Status = r.Status,
                PublishedAt = r.PublishedAt,
                AuthorId = r.AuthorId,
                CategoryId = r.CategoryId,
                Version = r.Version,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Nutrition = r.Nutrition == null ? null : new RecipeNutritionDto
                {
                    Calories = r.Nutrition.Calories,
                    ProteinGrams = r.Nutrition.ProteinGrams,
                    FatGrams = r.Nutrition.FatGrams,
                    CarbsGrams = r.Nutrition.CarbsGrams,
                    FiberGrams = r.Nutrition.FiberGrams,
                    SugarGrams = r.Nutrition.SugarGrams
                },
                Steps = r.Steps.OrderBy(s => s.StepNumber).Select(s => new RecipeStepDto
                {
                    Id = s.Id,
                    StepNumber = s.StepNumber,
                    Title = s.Title,
                    Description = s.Description,
                    DurationMinutes = s.DurationMinutes
                }).ToList(),
                Ingredients = r.Ingredients.OrderBy(i => i.OrderIndex).Select(i => new RecipeIngredientDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    OrderIndex = i.OrderIndex
                }).ToList()
            })
            .ToList();

        var result = new PagedResult<RecipeDto>(items, page, pageSize, totalCount);

        return Task.FromResult(result);
    }
}
