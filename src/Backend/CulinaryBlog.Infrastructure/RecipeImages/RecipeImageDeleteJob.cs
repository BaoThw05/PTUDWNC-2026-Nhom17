using CulinaryBlog.Application.Abstractions;

namespace CulinaryBlog.Infrastructure.RecipeImages;

internal sealed class RecipeImageDeleteJob(IFileStorage storage) : IRecipeImageDeleteJob
{
    public Task ExecuteAsync(string prefix) => storage.DeleteByPrefixAsync(prefix);
}
