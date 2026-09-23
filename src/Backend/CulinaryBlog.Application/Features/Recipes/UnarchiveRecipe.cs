using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record UnarchiveRecipeCommand(Guid Id) : IRequest;

public sealed class UnarchiveRecipeCommandHandler(IAppDbContext db) : IRequestHandler<UnarchiveRecipeCommand>
{
    public async Task Handle(UnarchiveRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        if (recipe.Status == RecipeStatus.Draft)
        {
            return; // idempotent (S-11)
        }

        recipe.Status = RecipeStatus.Draft;

        await db.SaveChangesAsync(cancellationToken);
    }
}
