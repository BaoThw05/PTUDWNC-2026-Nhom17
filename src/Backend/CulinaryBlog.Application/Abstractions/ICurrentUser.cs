namespace CulinaryBlog.Application.Abstractions;

/// <summary>
/// Thông tin người gọi request hiện tại, lấy từ JWT và kết nối HTTP.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Null khi request chưa đăng nhập.</summary>
    Guid? UserId { get; }

    string? IpAddress { get; }

    /// <summary>Thiết bị/trình duyệt gửi request; mặc định null để các fake trong test không phải cài đặt.</summary>
    string? UserAgent => null;

    bool IsInRole(string role);
}
