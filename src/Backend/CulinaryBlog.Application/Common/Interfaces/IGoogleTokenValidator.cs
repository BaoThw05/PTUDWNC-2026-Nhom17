namespace CulinaryBlog.Application.Common.Interfaces;

public record GoogleProfile(string ProviderKey, string Email, bool EmailVerified, string? FullName, string? AvatarUrl);

public interface IGoogleTokenValidator
{
    /// <summary>
    /// Xác thực Google ID token (chữ ký, issuer, audience, hạn dùng) và trả về thông tin
    /// profile đã được Google xác nhận. Ném AppUnauthorizedException nếu token không hợp
    /// lệ/hết hạn, ExternalServiceException nếu không gọi được Google.
    /// </summary>
    Task<GoogleProfile> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
