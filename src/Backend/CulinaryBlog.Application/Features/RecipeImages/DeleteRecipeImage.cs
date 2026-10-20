using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes;
using MediatR;

namespace CulinaryBlog.Application.Features.RecipeImages;

public sealed record DeleteRecipeImageCommand(Guid RecipeId, Guid ImageId) : IRequest;

public sealed class DeleteRecipeImageCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorization,
    IBackgroundJobService jobs,
    ICacheInvalidator cache) : IRequestHandler<DeleteRecipeImageCommand>
{
    public async Task Handle(DeleteRecipeImageCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(item => item.Id == request.RecipeId)
            ?? throw new NotFoundException($"Không tìm thấy công thức có id '{request.RecipeId}'.", "RECIPE_NOT_FOUND");
        authorization.EnsureCanModify(recipe);

        var image = db.RecipeImages.FirstOrDefault(item => item.Id == request.ImageId && item.RecipeId == request.RecipeId)
            ?? throw new NotFoundException($"Không tìm thấy ảnh có id '{request.ImageId}'.", "IMAGE_NOT_FOUND");

        var nextPrimary = image.IsPrimary
            ? db.RecipeImages.Where(item => item.RecipeId == request.RecipeId && item.Id != image.Id)
                .OrderBy(item => item.OrderIndex).ThenBy(item => item.Id).FirstOrDefault()
            : null;

        async Task DeleteAsync()
        {
            db.Remove(image);
            recipe.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            if (nextPrimary is not null)
            {
                nextPrimary.IsPrimary = true;
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        if (nextPrimary is not null)
            await db.ExecuteInTransactionAsync(DeleteAsync, cancellationToken);
        else
            await DeleteAsync();

        jobs.Enqueue<IRecipeImageDeleteJob>(job => job.ExecuteAsync($"recipes/{request.RecipeId}/{request.ImageId}"));
        await cache.InvalidateAsync(["recipes"], cancellationToken);
    }
}
