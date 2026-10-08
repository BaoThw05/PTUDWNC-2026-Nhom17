using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using CulinaryBlog.Domain.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth.Refresh;

/// <summary>
/// Xoay vòng refresh token theo S-06: token vừa bị thay trong 30 giây vẫn đổi được;
/// dùng lại sau thời gian đó thì thu hồi cả family.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IUserAccountService users,
    AuthSessionIssuer sessions,
    TimeProvider timeProvider,
    ILogger<RefreshTokenCommandHandler> logger) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    // Hai tab refresh cùng lúc: request thua sẽ đọc lại token (lúc này đã bị xoay vòng) và đi nhánh ân hạn.
    private const int MaxAttempts = 2;

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenValue.Hash(request.RefreshToken);

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var response = await TryRefreshAsync(tokenHash, cancellationToken);
            if (response is not null)
            {
                return response;
            }
        }

        throw Revoked();
    }

    private async Task<AuthResponse?> TryRefreshAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var token = await refreshTokens.FindByHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedException("The refresh token is invalid.", AuthErrorCodes.RefreshTokenInvalid);
        var now = timeProvider.GetUtcNow();

        if (token.IsRevoked)
        {
            return await HandleRevokedAsync(token, now, cancellationToken);
        }

        if (token.IsExpired(now))
        {
            throw new UnauthorizedException("The refresh token has expired.", AuthErrorCodes.RefreshTokenExpired);
        }

        var user = await GetActiveUserAsync(token.UserId, cancellationToken);
        var response = sessions.Continue(user, token.FamilyId, out var replacement);
        token.Revoke(RefreshTokenRevokeReason.Rotated, now, replacement.TokenHash);

        return await refreshTokens.TrySaveChangesAsync(cancellationToken) ? response : null;
    }

    private async Task<AuthResponse?> HandleRevokedAsync(
        RefreshToken token,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (token.IsWithinReuseGracePeriod(now))
        {
            var user = await GetActiveUserAsync(token.UserId, cancellationToken);
            var response = sessions.Continue(user, token.FamilyId, out _);
            return await refreshTokens.TrySaveChangesAsync(cancellationToken) ? response : null;
        }

        if (token.RevokedReason == RefreshTokenRevokeReason.Rotated)
        {
            logger.LogWarning(
                "Security alert: reuse of rotated refresh token detected for user {UserId}; revoking token family {FamilyId}",
                token.UserId,
                token.FamilyId);

            var family = await refreshTokens.GetActiveInFamilyAsync(token.FamilyId, now, cancellationToken);
            foreach (var member in family)
            {
                member.Revoke(RefreshTokenRevokeReason.ReuseDetected, now);
            }

            await refreshTokens.TrySaveChangesAsync(cancellationToken);
        }

        throw Revoked();
    }

    private async Task<UserAccount> GetActiveUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException("The refresh token is invalid.", AuthErrorCodes.RefreshTokenInvalid);

        return user.IsActive
            ? user
            : throw new ForbiddenException("The account is disabled.", AuthErrorCodes.AccountDisabled);
    }

    private static UnauthorizedException Revoked() =>
        new("The refresh token has been revoked.", AuthErrorCodes.RefreshTokenRevoked);
}
