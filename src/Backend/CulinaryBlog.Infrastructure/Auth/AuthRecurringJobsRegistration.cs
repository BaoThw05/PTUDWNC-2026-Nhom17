using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.RefreshTokenCleanup;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Auth;

/// <summary>Đăng ký job định kỳ của module Auth khi API khởi động (việc 1.20).</summary>
/// <remarks><paramref name="jobs"/> là null khi không có connection string (Hangfire không được đăng ký, ví dụ test không DB).</remarks>
internal sealed class AuthRecurringJobsRegistration(
    ILogger<AuthRecurringJobsRegistration> logger,
    IBackgroundJobService? jobs = null) : IHostedService
{
    public const string RefreshTokenCleanupJobId = "auth-refresh-token-cleanup";

    // Hằng ngày lúc 03:00 UTC (10:00 giờ Việt Nam), ngoài giờ cao điểm.
    private const string DailyAt3AmUtc = "0 3 * * *";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (jobs is null)
        {
            return Task.CompletedTask;
        }

        try
        {
            jobs.AddOrUpdateRecurring<RefreshTokenCleanupJob>(
                RefreshTokenCleanupJobId,
                job => job.ExecuteAsync(CancellationToken.None),
                DailyAt3AmUtc);
        }
        catch (Exception exception)
        {
            // Giống DatabaseInitializer: DB chưa chạy thì chỉ ghi log, API vẫn lên.
            logger.LogError(exception, "Could not register recurring job {JobId}", RefreshTokenCleanupJobId);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
