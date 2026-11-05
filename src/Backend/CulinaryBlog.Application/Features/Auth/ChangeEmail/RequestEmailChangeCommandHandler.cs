using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ChangeEmail;

/// <summary>
/// Mật khẩu sai được đếm vào lockout như khi đăng nhập. Tài khoản chỉ đăng nhập bằng Google (chưa có mật khẩu)
/// dùng "Quên mật khẩu" để đặt mật khẩu trước.
/// </summary>
public sealed class RequestEmailChangeCommandHandler(
    ICurrentUser currentUser,
    IUserAccountService users,
    IBackgroundJobService jobs) : IRequestHandler<RequestEmailChangeCommand>
{
    public async Task Handle(RequestEmailChangeCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("Authentication is required.");
        var newEmail = request.NewEmail.Trim();

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

        var owner = await users.FindByEmailAsync(newEmail, cancellationToken);
        if (owner?.Id == userId)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["NewEmail"] = ["'New Email' must be different from the current email."],
            });
        }

        if (owner is not null)
        {
            throw new ConflictException("The email is already registered.", AuthErrorCodes.EmailExists);
        }

        jobs.Enqueue<IAccountEmailJob>(job => job.SendChangeEmailCodeAsync(userId, newEmail, CancellationToken.None));
    }
}
