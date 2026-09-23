using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record ArchiveRecipeCommand(Guid Id) : IRequest;

public sealed class ArchiveRecipeCommandHandler(IAppDbContext db) : IRequestHandler<ArchiveRecipeCommand>
{
    public async Task Handle(ArchiveRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        if (recipe.Status == RecipeStatus.Archived)
        {
            return; // idempotent (S-11)
        }

        recipe.Status = RecipeStatus.Archived;

        await db.SaveChangesAsync(cancellationToken);
    }
}
