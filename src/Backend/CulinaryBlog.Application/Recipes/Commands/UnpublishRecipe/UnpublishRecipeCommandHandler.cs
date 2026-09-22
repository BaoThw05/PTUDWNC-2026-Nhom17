using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Recipes.Commands.UnpublishRecipe;

public class UnpublishRecipeCommandHandler : IRequestHandler<UnpublishRecipeCommand>
{
    private readonly IApplicationDbContext _context;

    public UnpublishRecipeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UnpublishRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (recipe is null)
        {
            throw new NotFoundException("RECIPE_NOT_FOUND", $"Không tìm thấy công thức có id '{request.Id}'");
        }

        if (recipe.Status == RecipeStatus.Draft)
        {
            return; // idempotent
        }

        recipe.Status = RecipeStatus.Draft;

        await _context.SaveChangesAsync(cancellationToken);
    }
}