using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.Application.Features.Auth.Abstractions;

/// <summary>
/// Thao tác tài khoản người dùng; cài đặt bằng ASP.NET Core Identity ở Infrastructure (S-13).
/// </summary>
public interface IUserAccountService
{
    Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<UserAccount?> FindByExternalLoginAsync(ExternalLogin login, CancellationToken cancellationToken);

    /// <summary>Tạo tài khoản với vai trò Author. Email trùng → <c>ConflictException</c>; mật khẩu không đạt → <c>ValidationException</c>.</summary>
    Task<UserAccount> CreateAsync(NewUserAccount account, CancellationToken cancellationToken);

    /// <summary>Kiểm tra mật khẩu, tự đếm số lần sai và khóa tài khoản theo cấu hình lockout.</summary>
    Task<PasswordCheckResult> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken);

    /// <summary>Đặt mật khẩu mới (đã kiểm tra mật khẩu hiện tại); mật khẩu không đạt → <c>ValidationException</c>.</summary>
    Task SetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken);

    Task<UserAccount> UpdateFullNameAsync(Guid userId, string fullName, CancellationToken cancellationToken);

    /// <summary>Liên kết đăng nhập ngoài; ảnh đại diện chỉ được gán khi tài khoản chưa có (S-17).</summary>
    Task<UserAccount> LinkExternalLoginAsync(
        Guid userId,
        ExternalLogin login,
        string? avatarUrl,
        CancellationToken cancellationToken);

    /// <summary>Danh sách người dùng cho Admin, mới nhất trước; <paramref name="search"/> lọc theo email, tên đăng nhập, họ tên.</summary>
    Task<PagedResult<UserAccount>> ListAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Admin bật/tắt tài khoản và đặt lại vai trò; tham số null thì giữ nguyên.</summary>
    Task<UserAccount> UpdateAccessAsync(
        Guid userId,
        bool? isActive,
        IReadOnlyList<string>? roles,
        CancellationToken cancellationToken);
}
