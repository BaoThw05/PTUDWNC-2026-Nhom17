using CulinaryBlog.Application.Abstractions;

namespace CulinaryBlog.Infrastructure.Services;

public sealed class DefaultCategoryValidator : ICategoryValidator
{
    // Cài đặt mặc định cho đến khi TV3 hoàn thành Module Categories (FR-CAT-001)
    public Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        if (categoryId == Guid.Empty)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
