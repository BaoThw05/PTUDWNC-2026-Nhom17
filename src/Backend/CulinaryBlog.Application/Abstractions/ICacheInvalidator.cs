namespace CulinaryBlog.Application.Abstractions;

public interface ICacheInvalidator
{
    Task InvalidateAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
}
