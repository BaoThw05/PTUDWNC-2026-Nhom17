using CulinaryBlog.Application.Abstractions;

namespace CulinaryBlog.Infrastructure.RecipeImages;

internal sealed class RecipeImageResizeJob : IRecipeImageResizeJob
{
    public Task ExecuteAsync(Guid imageId) => Task.CompletedTask;
}
