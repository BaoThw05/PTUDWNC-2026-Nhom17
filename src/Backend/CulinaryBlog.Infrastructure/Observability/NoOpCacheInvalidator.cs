using CulinaryBlog.Application.Abstractions;

namespace CulinaryBlog.Infrastructure.Observability;

internal sealed class NoOpCacheInvalidator : ICacheInvalidator
{
    public Task InvalidateAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
