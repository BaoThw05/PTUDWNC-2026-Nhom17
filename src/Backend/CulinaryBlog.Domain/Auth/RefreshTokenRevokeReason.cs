namespace CulinaryBlog.Domain.Auth;

public enum RefreshTokenRevokeReason
{
    Rotated,
    Logout,
    ReuseDetected,
}
