using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Auth;

/// <summary>Một lần đăng nhập (thành công hoặc thất bại) của tài khoản đã tồn tại, để người dùng tự phát hiện truy cập lạ.</summary>
public sealed class LoginAttempt : BaseEntity
{
    public const int IpAddressMaxLength = 45;
    public const int UserAgentMaxLength = 512;

    private LoginAttempt()
    {
    }

    public Guid UserId { get; private set; }

    public LoginMethod Method { get; private set; }

    public LoginResult Result { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public static LoginAttempt Record(
        Guid userId,
        LoginMethod method,
        LoginResult result,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset now) =>
        new()
        {
            UserId = userId,
            Method = method,
            Result = result,
            IpAddress = ipAddress,
            UserAgent = userAgent is { Length: > UserAgentMaxLength } ? userAgent[..UserAgentMaxLength] : userAgent,
            CreatedAt = now,
        };
}

public enum LoginMethod
{
    Password,
    Google,
}

public enum LoginResult
{
    Succeeded,
    InvalidPassword,
    LockedOut,
    Disabled,
}
