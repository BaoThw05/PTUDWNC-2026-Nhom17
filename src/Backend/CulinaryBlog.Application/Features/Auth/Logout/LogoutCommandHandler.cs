using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using CulinaryBlog.Domain.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Logout;

/// <summary>
/// Đăng xuất luôn thành công để không lộ trạng thái token (FR-AUTH-005, S-05).
/// Nếu request có access token thì chỉ thu hồi khi refresh token thuộc cùng người dùng.
/// </summary>
public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokens,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await refreshTokens.FindByHashAsync(RefreshTokenValue.Hash(request.RefreshToken), cancellationToken);
        if (token is null || token.IsRevoked)
        {
            return;
        }

        if (currentUser.UserId is { } userId && userId != token.UserId)
        {
            return;
        }

        token.Revoke(RefreshTokenRevokeReason.Logout, timeProvider.GetUtcNow());
        await refreshTokens.TrySaveChangesAsync(cancellationToken);
    }
}
