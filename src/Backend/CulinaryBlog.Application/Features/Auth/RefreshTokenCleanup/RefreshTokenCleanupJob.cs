using CulinaryBlog.Application.Features.Auth.Abstractions;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth.RefreshTokenCleanup;

/// <summary>
/// Job chạy hằng ngày: xóa refresh token đã hết hạn quá <see cref="Retention"/> (việc 1.20).
/// TODO(TV1): đăng ký recurring (cron <c>Cron.Daily</c>) qua <c>IBackgroundJobService.AddOrUpdateRecurring</c> khi TV3 merge Hangfire (3.07).
/// </summary>
public sealed class RefreshTokenCleanupJob(
    IRefreshTokenRepository refreshTokens,
    TimeProvider timeProvider,
    ILogger<RefreshTokenCleanupJob> logger)
{
    public static readonly TimeSpan Retention = TimeSpan.FromDays(30);

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = timeProvider.GetUtcNow() - Retention;
        var deleted = await refreshTokens.DeleteExpiredBeforeAsync(cutoff, cancellationToken);
        logger.LogInformation("RefreshTokenCleanupJob: đã xóa {Count} refresh token hết hạn trước {Cutoff}", deleted, cutoff);
    }
}
