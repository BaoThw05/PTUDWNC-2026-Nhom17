namespace CulinaryBlog.Application.Abstractions;

public interface ICurrentUser
{
    string? UserId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
