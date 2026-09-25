namespace CulinaryBlog.Application.Abstractions;

/// <summary>
/// Thông tin người gọi request hiện tại, lấy từ JWT và kết nối HTTP.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Null khi request chưa đăng nhập.</summary>
    Guid? UserId { get; }

    string? IpAddress { get; }

    bool IsInRole(string role);
}
