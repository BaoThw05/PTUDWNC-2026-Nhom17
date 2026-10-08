using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ChangePassword;

/// <summary>
/// Đổi mật khẩu rồi thu hồi mọi refresh token của người dùng, kể cả thiết bị hiện tại: người dùng đăng nhập lại bằng mật khẩu mới.
/// Mật khẩu hiện tại sai được đếm vào lockout như khi đăng nhập.
/// </summary>
public sealed class ChangePasswordCommandHandler(
    ICurrentUser currentUser,
    IUserAccountService users,
    IRefreshTokenRepository refreshTokens,
    TimeProvider timeProvider) : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("Authentication is required.");

        switch (await users.CheckPasswordAsync(userId, request.CurrentPassword, cancellationToken))
        {
            case PasswordCheckResult.LockedOut:
                throw new AccountLockedException();
            case PasswordCheckResult.InvalidPassword:
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["CurrentPassword"] = ["The current password is incorrect."],
                });
        }

        await users.SetPasswordAsync(userId, request.NewPassword, cancellationToken);

        var now = timeProvider.GetUtcNow();
        foreach (var token in await refreshTokens.GetActiveForUserAsync(userId, now, cancellationToken))
        {
            token.Revoke(RefreshTokenRevokeReason.PasswordChanged, now);
        }

        await refreshTokens.TrySaveChangesAsync(cancellationToken);
    }
}
