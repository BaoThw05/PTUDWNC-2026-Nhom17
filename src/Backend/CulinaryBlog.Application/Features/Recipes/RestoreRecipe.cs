using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record RestoreRecipeCommand(Guid Id) : IRequest;

public sealed class RestoreRecipeCommandHandler(IAppDbContext db) : IRequestHandler<RestoreRecipeCommand>
{
    public async Task Handle(RestoreRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.RecipesIncludingDeleted.FirstOrDefault(r => r.Id == request.Id && r.IsDeleted)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức đã xóa có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        var slugTaken = db.Recipes.Any(r => r.Slug == recipe.Slug && r.Id != recipe.Id);

        if (slugTaken)
        {
            recipe.Slug = $"{recipe.Slug}-{Guid.NewGuid().ToString()[..6]}";
        }

        recipe.IsDeleted = false;
        recipe.DeletedAt = null;

        await db.SaveChangesAsync(cancellationToken);
    }
}
