using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record DeleteRecipeCommand(Guid Id) : IRequest;

public sealed class DeleteRecipeCommandHandler(IAppDbContext db) : IRequestHandler<DeleteRecipeCommand>
{
    public async Task Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        recipe.IsDeleted = true;
        recipe.DeletedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
