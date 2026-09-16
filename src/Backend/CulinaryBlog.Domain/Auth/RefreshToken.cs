using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Auth;

/// <summary>
/// Refresh token đã phát hành (S-06). Chỉ lưu SHA-256 của token; các token xoay vòng từ cùng một lần đăng nhập chung <see cref="FamilyId"/>.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    /// <summary>Token vừa bị xoay vòng mà được dùng lại trong khoảng này vẫn được cấp token mới (nhiều tab gọi refresh cùng lúc).</summary>
    public static readonly TimeSpan ReuseGracePeriod = TimeSpan.FromSeconds(30);

    private RefreshToken()
    {
    }

    public Guid UserId { get; private set; }

    public Guid FamilyId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public RefreshTokenRevokeReason? RevokedReason { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public string? CreatedByIp { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    public static RefreshToken Issue(Guid userId, Guid familyId, string tokenHash, DateTimeOffset now, string? createdByIp) =>
        new()
        {
            UserId = userId,
            FamilyId = familyId,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = now + Lifetime,
            CreatedByIp = createdByIp,
        };

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);

    public bool IsWithinReuseGracePeriod(DateTimeOffset now) =>
        RevokedReason == RefreshTokenRevokeReason.Rotated && now - RevokedAt <= ReuseGracePeriod;

    public void Revoke(RefreshTokenRevokeReason reason, DateTimeOffset now, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAt = now;
        RevokedReason = reason;
        ReplacedByTokenHash = replacedByTokenHash;
        UpdatedAt = now;
    }
}
