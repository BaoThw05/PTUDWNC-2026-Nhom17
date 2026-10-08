namespace CulinaryBlog.Application.Features.Auth.Abstractions;

public enum PasswordCheckResult
{
    Success,
    InvalidPassword,
    LockedOut,
}
