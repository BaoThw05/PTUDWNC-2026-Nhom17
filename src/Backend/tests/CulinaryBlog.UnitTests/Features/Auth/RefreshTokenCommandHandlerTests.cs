using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Common;
using CulinaryBlog.Application.Features.Auth.Refresh;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly AuthTestContext _context = new();

    [Fact]
    public async Task Handle_ActiveToken_RotatesWithinSameFamily()
    {
        var session = await StartSessionAsync();

        var response = await RefreshAsync(session.RefreshToken);

        var oldToken = FindToken(session.RefreshToken);
        var newToken = FindToken(response.RefreshToken);
        Assert.Equal(RefreshTokenRevokeReason.Rotated, oldToken.RevokedReason);
        Assert.Equal(newToken.TokenHash, oldToken.ReplacedByTokenHash);
        Assert.Equal(oldToken.FamilyId, newToken.FamilyId);
        Assert.False(newToken.IsRevoked);
    }

    [Fact]
    public async Task Handle_RotatedTokenReusedWithinGracePeriod_IssuesNewToken()
    {
        var session = await StartSessionAsync();
        await RefreshAsync(session.RefreshToken);
        _context.Time.Advance(TimeSpan.FromSeconds(10));

        var response = await RefreshAsync(session.RefreshToken);

        Assert.False(FindToken(response.RefreshToken).IsRevoked);
        Assert.DoesNotContain(_context.RefreshTokens.Tokens, token => token.RevokedReason == RefreshTokenRevokeReason.ReuseDetected);
    }

    [Fact]
    public async Task Handle_RotatedTokenReusedAfterGracePeriod_RevokesWholeFamily()
    {
        var session = await StartSessionAsync();
        var rotated = await RefreshAsync(session.RefreshToken);
        _context.Time.Advance(TimeSpan.FromMinutes(1));

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => RefreshAsync(session.RefreshToken));

        Assert.Equal(AuthErrorCodes.RefreshTokenRevoked, exception.Code);
        Assert.Equal(RefreshTokenRevokeReason.ReuseDetected, FindToken(rotated.RefreshToken).RevokedReason);
    }

    [Fact]
    public async Task Handle_LoggedOutToken_ThrowsRevokedWithoutRevokingFamily()
    {
        var first = await StartSessionAsync();
        var token = FindToken(first.RefreshToken);
        token.Revoke(RefreshTokenRevokeReason.Logout, _context.Time.GetUtcNow());

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => RefreshAsync(first.RefreshToken));

        Assert.Equal(AuthErrorCodes.RefreshTokenRevoked, exception.Code);
        Assert.DoesNotContain(_context.RefreshTokens.Tokens, t => t.RevokedReason == RefreshTokenRevokeReason.ReuseDetected);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsExpired()
    {
        var session = await StartSessionAsync();
        _context.Time.Advance(RefreshToken.Lifetime);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => RefreshAsync(session.RefreshToken));

        Assert.Equal(AuthErrorCodes.RefreshTokenExpired, exception.Code);
    }

    [Fact]
    public async Task Handle_UnknownToken_ThrowsInvalid()
    {
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => RefreshAsync("unknown"));

        Assert.Equal(AuthErrorCodes.RefreshTokenInvalid, exception.Code);
    }

    [Fact]
    public async Task Handle_DisabledUser_ThrowsForbidden()
    {
        var session = await StartSessionAsync();
        var user = await _context.Users.FindByIdAsync(session.User.Id, CancellationToken.None);
        _context.Users.Replace(user! with { IsActive = false });

        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => RefreshAsync(session.RefreshToken));

        Assert.Equal(AuthErrorCodes.AccountDisabled, exception.Code);
    }

    [Fact]
    public async Task Handle_ConcurrentRotation_RetriesThroughGracePeriod()
    {
        var session = await StartSessionAsync();
        _context.RefreshTokens.ConflictsToSimulate = 1;

        var response = await RefreshAsync(session.RefreshToken);

        Assert.False(FindToken(response.RefreshToken).IsRevoked);
    }

    private async Task<AuthResponse> StartSessionAsync()
    {
        var user = _context.Users.Add("author@example.com", "Author@12345");
        return await _context.Sessions.StartAsync(user, CancellationToken.None);
    }

    private Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var handler = new RefreshTokenCommandHandler(
            _context.RefreshTokens,
            _context.Users,
            _context.Sessions,
            _context.Time,
            NullLogger<RefreshTokenCommandHandler>.Instance);

        return handler.Handle(new RefreshTokenCommand(refreshToken), CancellationToken.None);
    }

    private RefreshToken FindToken(string plainToken) =>
        _context.RefreshTokens.Tokens.Single(token => token.TokenHash == RefreshTokenValue.Hash(plainToken));
}
