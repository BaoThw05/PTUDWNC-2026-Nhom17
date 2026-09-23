namespace CulinaryBlog.Application.Abstractions;

public interface ICategoryValidator
{
    Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
