namespace CulinaryBlog.Application.Abstractions;

public interface IRecipeImageResizeJob
{
    Task ExecuteAsync(Guid imageId);
}
