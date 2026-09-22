using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Recipes.Commands.UpdateRecipe;

public class UpdateRecipeCommandHandler : IRequestHandler<UpdateRecipeCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateRecipeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (recipe is null)
        {
            throw new NotFoundException("RECIPE_NOT_FOUND", $"Không tìm th?y công th?c có id '{request.Id}'");
        }

        recipe.Title = request.Title;
        recipe.Description = request.Description;
        recipe.PrepTimeMinutes = request.PrepTimeMinutes;
        recipe.CookTimeMinutes = request.CookTimeMinutes;
        recipe.Servings = request.Servings;
        recipe.Difficulty = request.Difficulty;

        // Gán l?i giá tr? version client g?i lên - EF Core s? so sánh v?i xmin th?t trong DB
        // khi UPDATE; n?u l?ch, ném DbUpdateConcurrencyException (GlobalExceptionHandler b?t và tr? 409)
        _context.Entry(recipe).Property(r => r.Version).OriginalValue = request.Version;

        await _context.SaveChangesAsync(cancellationToken);
    }
}