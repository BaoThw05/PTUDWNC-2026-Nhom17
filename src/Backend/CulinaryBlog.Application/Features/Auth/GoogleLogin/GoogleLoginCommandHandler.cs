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
            new NewUserAccount(identity.Email, DisplayNameOf(identity), Password: null, identity.PictureUrl),
            cancellationToken);

        // TODO(TV1): gửi email chào mừng cho tài khoản mới tạo từ Google (việc 1.15).
        return await users.LinkExternalLoginAsync(created.Id, login, avatarUrl: null, cancellationToken);
    }

    private static string DisplayNameOf(GoogleIdentity identity)
    {
        var name = identity.Name?.Trim();
        if (name is { Length: >= AuthValidationRules.DisplayNameMinLength })
        {
            return name.Length > AuthValidationRules.DisplayNameMaxLength
                ? name[..AuthValidationRules.DisplayNameMaxLength]
                : name;
        }

        return identity.Email.Split('@')[0].PadRight(AuthValidationRules.DisplayNameMinLength, '_');
    }
}
