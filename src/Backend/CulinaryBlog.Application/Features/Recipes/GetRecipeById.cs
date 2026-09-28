using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record GetRecipeByIdQuery(Guid Id) : IRequest<RecipeDto>;

public sealed class GetRecipeByIdQueryHandler(IAppDbContext db, IRecipeAuthorizationHandler authorizationHandler)
    : IRequestHandler<GetRecipeByIdQuery, RecipeDto>
{
    public Task<RecipeDto> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        var recipe = db.RecipesIncludingDeleted
            .Where(r => r.Id == request.Id)
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
            .FirstOrDefault();

        if (recipe is null)
        {
            throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);
        }

        // Quyết định S-11: Nếu bài chưa xuất bản hoặc đã bị xóa mềm,
        // chỉ tác giả hoặc Admin mới được phép xem; người ngoài truy cập nhận 404 (không tiết lộ sự tồn tại).
        authorizationHandler.EnsureCanView(recipe.Status, isDeleted: false, recipe.AuthorId, recipe.Id);

        return Task.FromResult(recipe);
    }
}
