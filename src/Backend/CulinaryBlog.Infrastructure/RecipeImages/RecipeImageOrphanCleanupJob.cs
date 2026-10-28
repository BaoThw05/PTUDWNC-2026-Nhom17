using CulinaryBlog.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.RecipeImages;

public sealed class RecipeImageOrphanCleanupJob(
    IAppDbContext db,
    IFileStorage storage,
    TimeProvider clock,
    ILogger<RecipeImageOrphanCleanupJob> logger) : IRecipeImageOrphanCleanupJob
{
    private static readonly TimeSpan MinimumAge = TimeSpan.FromHours(1);

    public async Task ExecuteAsync()
    {
        var cutoff = clock.GetUtcNow() - MinimumAge;
        var files = await storage.ListFilesAsync("recipes");
        var imageIds = db.RecipeImagesIncludingDeleted.Select(image => image.Id).ToHashSet();
        var trashedRecipeIds = db.RecipesIncludingDeleted
            .Where(recipe => recipe.IsDeleted)
            .Select(recipe => recipe.Id)
            .ToHashSet();

        foreach (var group in files.GroupBy(file => GetImagePrefix(file.Key)))
        {
            if (group.Key is null || group.Any(file => file.LastModified > cutoff))
            {
                continue;
            }

            var (recipeId, imageId, prefix) = group.Key.Value;
            if (imageIds.Contains(imageId) || trashedRecipeIds.Contains(recipeId))
            {
                continue;
            }

            // Đọc lại DB ngay trước khi xóa để hạn chế va chạm với upload đang chạy.
            if (db.RecipeImagesIncludingDeleted.Any(image => image.Id == imageId)
                || db.RecipesIncludingDeleted.Any(recipe => recipe.Id == recipeId && recipe.IsDeleted))
            {
                continue;
            }

            await storage.DeleteByPrefixAsync(prefix);
            logger.LogInformation("Deleted orphan recipe image files at {Prefix}", prefix);
        }
    }

    private static (Guid RecipeId, Guid ImageId, string Prefix)? GetImagePrefix(string key)
    {
        var parts = key.Split('/');
        if (parts.Length != 4 || parts[0] != "recipes"
            || !Guid.TryParse(parts[1], out var recipeId)
            || !Guid.TryParse(parts[2], out var imageId))
        {
            return null;
        }

        return (recipeId, imageId, $"recipes/{recipeId}/{imageId}");
    }
}
