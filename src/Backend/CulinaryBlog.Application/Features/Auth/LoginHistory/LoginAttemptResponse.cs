using CulinaryBlog.Domain.Auth;

namespace CulinaryBlog.Application.Features.Auth.LoginHistory;

public sealed record LoginAttemptResponse(
    DateTimeOffset OccurredAt,
    string Method,
    string Result,
    string? IpAddress,
    string? UserAgent)
{
    public static LoginAttemptResponse From(LoginAttempt attempt) =>
        new(attempt.CreatedAt, attempt.Method.ToString(), attempt.Result.ToString(), attempt.IpAddress, attempt.UserAgent);
}
