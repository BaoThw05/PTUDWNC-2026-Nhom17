namespace CulinaryBlog.Application.Abstractions;

public interface IRecipeImageDeleteJob
{
    Task ExecuteAsync(string prefix);
}
