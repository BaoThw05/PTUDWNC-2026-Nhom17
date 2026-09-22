using Microsoft.AspNetCore.Identity;

namespace CulinaryBlog.Infrastructure.Auth;

/// <summary>Người dùng Identity; đặt ở Infrastructure để Domain không phụ thuộc Identity (S-13).</summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
}
