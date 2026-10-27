using CulinaryBlog.Application.Features.Auth.Abstractions;

namespace CulinaryBlog.Application.Features.Auth.AdminUsers;

public sealed record AdminUserResponse(
    Guid Id,
    string Email,
    string UserName,
    string FullName,
    string? AvatarUrl,
    bool IsActive,
    IReadOnlyList<string> Roles,
    DateTimeOffset CreatedAt)
{
    public static AdminUserResponse From(UserAccount user) =>
        new(user.Id, user.Email, user.UserName, user.FullName, user.AvatarUrl, user.IsActive, user.Roles, user.CreatedAt);
}
