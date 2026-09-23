using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record GetRecipeByIdQuery(Guid Id) : IRequest<RecipeDto>;

public sealed class GetRecipeByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetRecipeByIdQuery, RecipeDto>
{
    public Task<RecipeDto> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes
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

        return Task.FromResult(recipe);
    }
}
