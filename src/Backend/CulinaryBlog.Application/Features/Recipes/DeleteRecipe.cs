using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record DeleteRecipeCommand(Guid Id) : IRequest;

public sealed class DeleteRecipeCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler,
    ICacheInvalidator cacheInvalidator,
    ILogger<DeleteRecipeCommandHandler> logger) : IRequestHandler<DeleteRecipeCommand>
{
    public async Task Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.15: Chỉ tác giả hoặc Admin mới có quyền xóa vào thùng rác
        authorizationHandler.EnsureCanModify(recipe);

        // Quyết định S-03: Xóa mềm vào thùng rác 30 ngày
        recipe.IsDeleted = true;
        recipe.DeletedAt = DateTimeOffset.UtcNow;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        // S-07 & 2.15: Xóa cache tag tương ứng
        await cacheInvalidator.InvalidateAsync(["recipes", $"recipe:{recipe.Id}"], cancellationToken);

        logger.LogInformation("Recipe {RecipeId} moved to trash (soft-deleted)", recipe.Id);
    }
}
