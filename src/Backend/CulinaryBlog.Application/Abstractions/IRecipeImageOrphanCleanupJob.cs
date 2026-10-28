namespace CulinaryBlog.Application.Abstractions;

public interface IRecipeImageOrphanCleanupJob
{
    Task ExecuteAsync();
}
