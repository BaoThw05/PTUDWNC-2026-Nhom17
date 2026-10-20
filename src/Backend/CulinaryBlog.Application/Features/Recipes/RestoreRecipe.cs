using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record RestoreRecipeCommand(Guid Id) : IRequest;

public sealed class RestoreRecipeCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler,
    ICacheInvalidator cacheInvalidator,
    ILogger<RestoreRecipeCommandHandler> logger) : IRequestHandler<RestoreRecipeCommand>
{
    public async Task Handle(RestoreRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.RecipesIncludingDeleted.FirstOrDefault(r => r.Id == request.Id && r.IsDeleted)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức đã xóa có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.15: Chỉ tác giả hoặc Admin mới có quyền khôi phục
        authorizationHandler.EnsureCanModify(recipe);

        // S-11 & 2.15: Nếu slug đã bị bài khác sử dụng trong lúc nằm trong thùng rác, tự động thêm hậu tố
        var baseSlug = recipe.Slug;
        var counter = 2;
        var candidateSlug = baseSlug;

        while (db.Recipes.Any(r => r.Id != recipe.Id && r.Slug == candidateSlug))
        {
            candidateSlug = $"{baseSlug}-{counter++}";
        }

        recipe.Slug = candidateSlug;
        recipe.IsDeleted = false;
        recipe.DeletedAt = null;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        // S-07 & 2.15: Xóa cache tag tương ứng
        await cacheInvalidator.InvalidateAsync(["recipes", $"recipe:{recipe.Id}"], cancellationToken);

        logger.LogInformation("Recipe {RecipeId} restored from trash with slug {Slug}", recipe.Id, recipe.Slug);
    }
}
