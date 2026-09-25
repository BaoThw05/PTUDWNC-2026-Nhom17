using CulinaryBlog.Application.Features.Auth.Abstractions;

namespace CulinaryBlog.Application.Features.Auth.Common;

public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string UserName,
    string FullName,
    string? AvatarUrl,
    IReadOnlyList<string> Roles,
    DateTimeOffset CreatedAt)
{
    public static UserProfileResponse From(UserAccount user) =>
        new(user.Id, user.Email, user.UserName, user.FullName, user.AvatarUrl, user.Roles, user.CreatedAt);
}
