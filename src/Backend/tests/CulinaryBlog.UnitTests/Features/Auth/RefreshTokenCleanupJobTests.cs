using CulinaryBlog.Application.Features.Auth.RefreshTokenCleanup;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class RefreshTokenCleanupJobTests
{
    private readonly AuthTestContext _context = new();

    [Fact]
    public async Task ExecuteAsync_DeletesOnlyTokensExpiredMoreThan30DaysAgo()
    {
        var issuedAt = _context.Time.GetUtcNow();
        _context.RefreshTokens.Add(RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "old", issuedAt, null));
        _context.Time.Advance(TimeSpan.FromDays(2));
        _context.RefreshTokens.Add(RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "recent", _context.Time.GetUtcNow(), null));

        // "old" hết hạn được 30 ngày + 1 giây; "recent" mới hết hạn 28 ngày.
        _context.Time.Advance(RefreshToken.Lifetime + RefreshTokenCleanupJob.Retention - TimeSpan.FromDays(2) + TimeSpan.FromSeconds(1));
        var job = new RefreshTokenCleanupJob(_context.RefreshTokens, _context.Time, NullLogger<RefreshTokenCleanupJob>.Instance);

        await job.ExecuteAsync(CancellationToken.None);

        Assert.Equal("recent", Assert.Single(_context.RefreshTokens.Tokens).TokenHash);
    }
}
