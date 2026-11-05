using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;

namespace CulinaryBlog.Application.Features.Auth.Common;

/// <summary>Ghi nhật ký đăng nhập kèm IP và thiết bị (User-Agent) của request hiện tại.</summary>
public sealed class LoginAudit(ILoginAttemptRepository attempts, ICurrentUser currentUser, TimeProvider timeProvider)
{
    public Task RecordAsync(Guid userId, LoginMethod method, LoginResult result, CancellationToken cancellationToken) =>
        attempts.AddAsync(
            LoginAttempt.Record(userId, method, result, currentUser.IpAddress, currentUser.UserAgent, timeProvider.GetUtcNow()),
            cancellationToken);
}
