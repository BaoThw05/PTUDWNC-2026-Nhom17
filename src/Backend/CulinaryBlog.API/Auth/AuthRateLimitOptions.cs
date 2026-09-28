namespace CulinaryBlog.API.Auth;

/// <summary>Hạn mức request mỗi phút (S-15).</summary>
public sealed class AuthRateLimitOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Đăng nhập, đăng ký, Google — theo IP.</summary>
    public int CredentialsPermitLimit { get; set; } = 5;

    public int RefreshPermitLimit { get; set; } = 30;

    /// <summary>Mọi request khác — theo người dùng nếu đã đăng nhập, ngược lại theo IP.</summary>
    public int GeneralPermitLimit { get; set; } = 100;

    /// <summary>Dải mạng của reverse proxy được tin cậy để đọc X-Forwarded-For (CIDR, ví dụ mạng Docker nội bộ).</summary>
    public string[] TrustedProxyNetworks { get; set; } = [];
}
