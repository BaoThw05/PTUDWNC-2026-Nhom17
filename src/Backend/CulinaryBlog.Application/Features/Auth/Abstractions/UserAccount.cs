namespace CulinaryBlog.Application.Features.Auth.Abstractions;

public sealed record UserAccount(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Roles);
