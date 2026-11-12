using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using CulinaryBlog.Domain.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(IUserAccountService users, AuthSessionIssuer sessions, LoginAudit audit)
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
                await audit.RecordAsync(user.Id, LoginMethod.Password, LoginResult.LockedOut, cancellationToken);
                throw new AccountLockedException();
            case PasswordCheckResult.InvalidPassword:
                await audit.RecordAsync(user.Id, LoginMethod.Password, LoginResult.InvalidPassword, cancellationToken);
                throw InvalidCredentials();
        }

        // Chỉ báo tài khoản bị vô hiệu hóa sau khi mật khẩu đúng.
        if (!user.IsActive)
        {
            await audit.RecordAsync(user.Id, LoginMethod.Password, LoginResult.Disabled, cancellationToken);
            throw new ForbiddenException("The account is disabled.", AuthErrorCodes.AccountDisabled);
        }

        var response = await sessions.StartAsync(user, cancellationToken);
        await audit.RecordAsync(user.Id, LoginMethod.Password, LoginResult.Succeeded, cancellationToken);
        return response;
    }

    private static UnauthorizedException InvalidCredentials() =>
        new("The email or password is incorrect.", AuthErrorCodes.InvalidCredentials);
}
