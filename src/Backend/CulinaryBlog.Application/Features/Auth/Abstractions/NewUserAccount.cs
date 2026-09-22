namespace CulinaryBlog.Application.Features.Auth.Abstractions;

/// <param name="Password">Null với tài khoản tạo từ Google.</param>
/// <param name="UserName">Google: sinh tự động từ email, không lấy từ người dùng (S-05).</param>
public sealed record NewUserAccount(
    string Email,
    string FullName,
    string UserName,
    string? Password,
    string? AvatarUrl = null);
