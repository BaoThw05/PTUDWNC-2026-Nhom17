using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;

namespace CulinaryBlog.Application.Features.Auth.Common;

/// <summary>Cấp cặp access token + refresh token cho một phiên đăng nhập.</summary>
public sealed class AuthSessionIssuer(
    IAccessTokenIssuer accessTokenIssuer,
    IRefreshTokenRepository refreshTokens,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    /// <summary>Mở phiên mới (đăng ký, đăng nhập): refresh token thuộc một family mới.</summary>
    public async Task<AuthResponse> StartAsync(UserAccount user, CancellationToken cancellationToken)
    {
        var response = Continue(user, Guid.CreateVersion7(), out _);
        await refreshTokens.TrySaveChangesAsync(cancellationToken);
        return response;
    }

    /// <summary>Cấp refresh token mới trong family đã có; người gọi tự lưu thay đổi.</summary>
    public AuthResponse Continue(UserAccount user, Guid familyId, out RefreshToken issued)
    {
        var plainToken = RefreshTokenValue.Generate();
        issued = RefreshToken.Issue(
            user.Id,
            familyId,
            RefreshTokenValue.Hash(plainToken),
            timeProvider.GetUtcNow(),
            currentUser.IpAddress);
        refreshTokens.Add(issued);

        var accessToken = accessTokenIssuer.Issue(user);

        return new AuthResponse(
            accessToken.Value,
            accessToken.ExpiresAt,
            plainToken,
            issued.ExpiresAt,
            UserProfileResponse.From(user));
    }
}
