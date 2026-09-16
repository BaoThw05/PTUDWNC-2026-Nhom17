using CulinaryBlog.Domain.Auth;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 20, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Issue_NewToken_ExpiresAfterSevenDays()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", Now, "127.0.0.1");

        Assert.Equal(Now.AddDays(7), token.ExpiresAt);
        Assert.True(token.IsActive(Now));
        Assert.False(token.IsActive(Now.AddDays(7)));
    }

    [Fact]
    public void Revoke_AlreadyRevoked_KeepsFirstReason()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", Now, null);

        token.Revoke(RefreshTokenRevokeReason.Logout, Now);
        token.Revoke(RefreshTokenRevokeReason.ReuseDetected, Now.AddMinutes(1));

        Assert.Equal(RefreshTokenRevokeReason.Logout, token.RevokedReason);
        Assert.Equal(Now, token.RevokedAt);
    }

    [Theory]
    [InlineData(RefreshTokenRevokeReason.Rotated, 30, true)]
    [InlineData(RefreshTokenRevokeReason.Rotated, 31, false)]
    [InlineData(RefreshTokenRevokeReason.Logout, 0, false)]
    public void IsWithinReuseGracePeriod_DependsOnReasonAndElapsedTime(
        RefreshTokenRevokeReason reason,
        int secondsAfterRevoke,
        bool expected)
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "hash", Now, null);
        token.Revoke(reason, Now);

        Assert.Equal(expected, token.IsWithinReuseGracePeriod(Now.AddSeconds(secondsAfterRevoke)));
    }
}
