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
}
