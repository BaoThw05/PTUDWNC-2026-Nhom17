using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ResetPassword;

/// <summary>Đặt lại mật khẩu bằng token trong email, rồi thu hồi mọi refresh token (giống đổi mật khẩu).</summary>
public sealed class ResetPasswordCommandHandler(
    IUserAccountService users,
    IRefreshTokenRepository refreshTokens,
    TimeProvider timeProvider) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // userId lạ trả cùng lỗi với token sai để không lộ tài khoản nào tồn tại.
        if (await users.FindByIdAsync(request.UserId, cancellationToken) is null)
        {
            throw new ValidationException(AuthErrorCodes.ResetTokenInvalid, "The password reset link is invalid or has expired.");
        }

        await users.ResetPasswordAsync(request.UserId, request.Token, request.NewPassword, cancellationToken);

        var now = timeProvider.GetUtcNow();
        foreach (var token in await refreshTokens.GetActiveForUserAsync(request.UserId, now, cancellationToken))
        {
            token.Revoke(RefreshTokenRevokeReason.PasswordChanged, now);
        }

        await refreshTokens.TrySaveChangesAsync(cancellationToken);
    }
}
