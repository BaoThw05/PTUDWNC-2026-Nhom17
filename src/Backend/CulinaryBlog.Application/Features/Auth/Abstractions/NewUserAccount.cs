namespace CulinaryBlog.Application.Features.Auth.Abstractions;

/// <param name="Password">Null với tài khoản tạo từ Google.</param>
public sealed record NewUserAccount(string Email, string DisplayName, string? Password, string? AvatarUrl = null);
