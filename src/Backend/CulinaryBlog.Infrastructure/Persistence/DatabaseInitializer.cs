using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>
/// Migrate và seed khi API khởi động (bật trong appsettings.Development.json).
/// DB chưa chạy thì chỉ ghi log lỗi, API vẫn lên để /health và các trang khác dùng được.
/// </summary>
internal sealed class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    IOptions<DatabaseOptions> options,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.ApplyMigrationsOnStartup && !settings.SeedOnStartup)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();

        try
        {
            if (settings.ApplyMigrationsOnStartup)
            {
                await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync(cancellationToken);
            }

            if (settings.SeedOnStartup)
            {
                foreach (var seeder in scope.ServiceProvider.GetServices<IDataSeeder>())
                {
                    await seeder.SeedAsync(cancellationToken);
                }
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Database initialization failed; check that PostgreSQL is running");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
