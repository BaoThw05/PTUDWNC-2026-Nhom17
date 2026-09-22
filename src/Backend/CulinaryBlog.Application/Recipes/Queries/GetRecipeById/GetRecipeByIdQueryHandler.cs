using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Recipes.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Recipes.Queries.GetRecipeById;

public class GetRecipeByIdQueryHandler : IRequestHandler<GetRecipeByIdQuery, RecipeDto>
{
    private readonly IApplicationDbContext _context;

    public GetRecipeByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RecipeDto> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Steps.OrderBy(s => s.StepNumber))
            .Include(r => r.Ingredients.OrderBy(i => i.OrderIndex))
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
                Steps = r.Steps.Select(s => new RecipeStepDto
                {
                    Id = s.Id,
                    StepNumber = s.StepNumber,
                    Title = s.Title,
                    Description = s.Description,
                    DurationMinutes = s.DurationMinutes
                }).ToList(),
                Ingredients = r.Ingredients.Select(i => new RecipeIngredientDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    OrderIndex = i.OrderIndex
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            throw new NotFoundException("RECIPE_NOT_FOUND", $"Không tìm th?y công th?c có id '{request.Id}'");
        }

        return recipe;
    }
}