using CulinaryBlog.Application.Features.Auth.Abstractions;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth.LoginHistory;

/// <summary>Job chạy hằng ngày: xóa nhật ký đăng nhập cũ hơn <see cref="Retention"/> để không giữ IP lâu hơn cần thiết.</summary>
public sealed class LoginAttemptCleanupJob(
    ILoginAttemptRepository attempts,
    TimeProvider timeProvider,
    ILogger<LoginAttemptCleanupJob> logger)
{
    public static readonly TimeSpan Retention = TimeSpan.FromDays(90);

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = timeProvider.GetUtcNow() - Retention;
        var deleted = await attempts.DeleteBeforeAsync(cutoff, cancellationToken);
        logger.LogInformation("LoginAttemptCleanupJob: đã xóa {Count} nhật ký đăng nhập trước {Cutoff}", deleted, cutoff);
    }
}
