using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.LoginHistory;
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
    public const string LoginAttemptCleanupJobId = "auth-login-attempt-cleanup";

    // Hằng ngày lúc 03:00 UTC (10:00 giờ Việt Nam), ngoài giờ cao điểm.
    private const string DailyAt3AmUtc = "0 3 * * *";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (jobs is null)
        {
            return Task.CompletedTask;
        }

        Register(RefreshTokenCleanupJobId, () => jobs.AddOrUpdateRecurring<RefreshTokenCleanupJob>(
            RefreshTokenCleanupJobId,
            job => job.ExecuteAsync(CancellationToken.None),
            DailyAt3AmUtc));
        Register(LoginAttemptCleanupJobId, () => jobs.AddOrUpdateRecurring<LoginAttemptCleanupJob>(
            LoginAttemptCleanupJobId,
            job => job.ExecuteAsync(CancellationToken.None),
            DailyAt3AmUtc));

        return Task.CompletedTask;
    }

    private void Register(string jobId, Action register)
    {
        try
        {
            register();
        }
        catch (Exception exception)
        {
            // Giống DatabaseInitializer: DB chưa chạy thì chỉ ghi log, API vẫn lên.
            logger.LogError(exception, "Could not register recurring job {JobId}", jobId);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
