using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Auth;

// Dùng Set<LoginAttempt>() (cấu hình nạp qua ApplyConfigurationsFromAssembly) để không phải sửa AppDbContext dùng chung.
internal sealed class LoginAttemptRepository(AppDbContext dbContext) : ILoginAttemptRepository
{
    private DbSet<LoginAttempt> Attempts => dbContext.Set<LoginAttempt>();

    public async Task AddAsync(LoginAttempt attempt, CancellationToken cancellationToken)
    {
        Attempts.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LoginAttempt>> ListRecentAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken) =>
        await Attempts.AsNoTracking()
            .Where(attempt => attempt.UserId == userId)
            .OrderByDescending(attempt => attempt.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> DeleteBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken) =>
        Attempts.Where(attempt => attempt.CreatedAt < cutoff).ExecuteDeleteAsync(cancellationToken);
}
