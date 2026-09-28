namespace CulinaryBlog.Application.Features.Auth;

/// <summary>Mã lỗi của module Auth; bảng đầy đủ ở docs/api/error-codes.md.</summary>
public static class AuthErrorCodes
{
    public const string EmailExists = "AUTH_EMAIL_EXISTS";
    public const string UserNameExists = "AUTH_USERNAME_EXISTS";
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AccountLocked = "AUTH_ACCOUNT_LOCKED";
    public const string AccountDisabled = "AUTH_ACCOUNT_DISABLED";
    public const string RefreshTokenInvalid = "AUTH_REFRESH_TOKEN_INVALID";
    public const string RefreshTokenExpired = "AUTH_REFRESH_TOKEN_EXPIRED";
    public const string RefreshTokenRevoked = "AUTH_REFRESH_TOKEN_REVOKED";
    public const string UserNotFound = "AUTH_USER_NOT_FOUND";
    public const string GoogleTokenInvalid = "AUTH_GOOGLE_TOKEN_INVALID";
    public const string GoogleEmailUnverified = "AUTH_GOOGLE_EMAIL_UNVERIFIED";
    public const string GoogleUnavailable = "AUTH_GOOGLE_UNAVAILABLE";
}
