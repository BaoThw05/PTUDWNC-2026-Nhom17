using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(IUserAccountService users, AuthSessionIssuer sessions)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Cùng một thông báo cho "không có email" và "sai mật khẩu" để không lộ email nào đã đăng ký.
        var user = await users.FindByEmailAsync(request.Email.Trim(), cancellationToken)
            ?? throw InvalidCredentials();

        var result = await users.CheckPasswordAsync(user.Id, request.Password, cancellationToken);
        switch (result)
        {
            case PasswordCheckResult.LockedOut:
                throw new AccountLockedException();
            case PasswordCheckResult.InvalidPassword:
                throw InvalidCredentials();
        }

        // Chỉ báo tài khoản bị vô hiệu hóa sau khi mật khẩu đúng.
        if (!user.IsActive)
        {
            throw new ForbiddenException("The account is disabled.", AuthErrorCodes.AccountDisabled);
        }

        return await sessions.StartAsync(user, cancellationToken);
    }

    private static UnauthorizedException InvalidCredentials() =>
        new("The email or password is incorrect.", AuthErrorCodes.InvalidCredentials);
}
