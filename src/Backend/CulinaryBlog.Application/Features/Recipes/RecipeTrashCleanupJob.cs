using CulinaryBlog.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Recipes;

/// <summary>
/// Định kỳ dọn dẹp các công thức trong thùng rác: recipe đã xóa mềm quá 30 ngày (<see cref="Retention"/>)
/// sẽ bị xóa vĩnh viễn khỏi CSDL và xóa toàn bộ file ảnh lưu trữ có prefix "recipes/{id}" (việc 2.21).
/// </summary>
public interface IRecipeTrashCleanupJob
{
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
}

public sealed class RecipeTrashCleanupJob(
    IAppDbContext db,
    IFileStorage fileStorage,
    TimeProvider timeProvider,
    ILogger<RecipeTrashCleanupJob> logger) : IRecipeTrashCleanupJob
{
    public static readonly TimeSpan Retention = TimeSpan.FromDays(30);

    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = timeProvider.GetUtcNow() - Retention;

        var expiredRecipes = db.RecipesIncludingDeleted
            .Where(r => r.IsDeleted && r.DeletedAt != null && r.DeletedAt <= cutoff)
            .ToList();

        if (expiredRecipes.Count == 0)
        {
            logger.LogInformation("RecipeTrashCleanupJob: Không có công thức nào quá 30 ngày trong thùng rác cần dọn");
            return 0;
        }

        foreach (var recipe in expiredRecipes)
        {
            // S-03 & 2.21: Xóa toàn bộ file ảnh lưu trữ liên quan đến recipe theo prefix "recipes/{id}"
            await fileStorage.DeleteByPrefixAsync($"recipes/{recipe.Id}", cancellationToken);

            // Xóa cứng khỏi CSDL (cascade delete)
            db.Remove(recipe);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "RecipeTrashCleanupJob: Đã xóa vĩnh viễn {Count} công thức trong thùng rác đã xóa trước {Cutoff}",
            expiredRecipes.Count,
            cutoff);

        return expiredRecipes.Count;
    }
}
