namespace CulinaryBlog.API.Auth;

/// <summary>
/// Tên policy dùng chung cho mọi module, ví dụ <c>group.RequireAuthorization(AuthPolicies.Author)</c>.
/// </summary>
public static class AuthPolicies
{
    /// <summary>Author hoặc Admin.</summary>
    public const string Author = "AuthorPolicy";

    public const string Admin = "AdminPolicy";
}
