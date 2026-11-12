using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;

namespace CulinaryBlog.UnitTests.Features.Auth.Fakes;

internal sealed class InMemoryLoginAttemptRepository : ILoginAttemptRepository
{
    public List<LoginAttempt> Attempts { get; } = [];

    public Task AddAsync(LoginAttempt attempt, CancellationToken cancellationToken)
    {
        Attempts.Add(attempt);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LoginAttempt>> ListRecentAsync(Guid userId, int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<LoginAttempt>>(
            [.. Attempts.Where(attempt => attempt.UserId == userId).OrderByDescending(attempt => attempt.CreatedAt).Take(take)]);

    public Task<int> DeleteBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken) =>
        Task.FromResult(Attempts.RemoveAll(attempt => attempt.CreatedAt < cutoff));
}
