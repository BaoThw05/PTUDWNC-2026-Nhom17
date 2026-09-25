namespace CulinaryBlog.Domain.Auth;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Author = "Author";

    public static readonly IReadOnlyList<string> All = [Admin, Author];
}
