using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.GoogleLogin;

public sealed class GoogleLoginCommandHandler(
    IGoogleIdTokenValidator googleTokens,
    IUserAccountService users,
    AuthSessionIssuer sessions) : IRequestHandler<GoogleLoginCommand, AuthResponse>
{
    public const string ProviderName = "Google";

    public async Task<AuthResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var identity = await googleTokens.ValidateAsync(request.IdToken, cancellationToken);

        // Chỉ liên kết với tài khoản có sẵn khi Google đã xác minh email, tránh chiếm tài khoản.
        if (!identity.EmailVerified)
        {
            throw new UnauthorizedException(
                "The Google account email is not verified.",
                AuthErrorCodes.GoogleEmailUnverified);
        }

        var login = new ExternalLogin(ProviderName, identity.Subject);
        var user = await users.FindByExternalLoginAsync(login, cancellationToken)
            ?? await LinkOrCreateAsync(identity, login, cancellationToken);

        if (!user.IsActive)
        {
            throw new ForbiddenException("The account is disabled.", AuthErrorCodes.AccountDisabled);
        }

        return await sessions.StartAsync(user, cancellationToken);
    }

    private async Task<UserAccount> LinkOrCreateAsync(
        GoogleIdentity identity,
        ExternalLogin login,
        CancellationToken cancellationToken)
    {
        var existing = await users.FindByEmailAsync(identity.Email, cancellationToken);
        if (existing is not null)
        {
            return await users.LinkExternalLoginAsync(existing.Id, login, identity.PictureUrl, cancellationToken);
        }

        var created = await users.CreateAsync(
            new NewUserAccount(identity.Email, FullNameOf(identity), GenerateUserNameFrom(identity.Email), Password: null, identity.PictureUrl),
            cancellationToken);

        // TODO(TV1): gửi email chào mừng cho tài khoản mới tạo từ Google (việc 1.15).
        return await users.LinkExternalLoginAsync(created.Id, login, avatarUrl: null, cancellationToken);
    }

    private static string FullNameOf(GoogleIdentity identity)
    {
        var name = identity.Name?.Trim();
        if (name is { Length: >= AuthValidationRules.FullNameMinLength })
        {
            return name.Length > AuthValidationRules.FullNameMaxLength
                ? name[..AuthValidationRules.FullNameMaxLength]
                : name;
        }

        return identity.Email.Split('@')[0].PadRight(AuthValidationRules.FullNameMinLength, '_');
    }

    // S-05: "sinh UserName tự động" cho tài khoản tạo từ Google.
    // ponytail: không kiểm tra trùng trước khi tạo; nếu username sinh ra đã tồn tại,
    // CreateAsync ném AUTH_USERNAME_EXISTS và người dùng phải thử lại đăng nhập Google —
    // thêm hậu tố ngẫu nhiên khi tần suất trùng thực tế đáng kể.
    private static string GenerateUserNameFrom(string email)
    {
        var local = new string([.. email.Split('@')[0].Where(char.IsLetterOrDigit)]);
        return local.Length >= AuthValidationRules.UserNameMinLength
            ? local
            : local.PadRight(AuthValidationRules.UserNameMinLength, '0');
    }
}
