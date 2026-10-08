using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Infrastructure.BackgroundJobs;
using Hangfire;
using Hangfire.PostgreSql;
using CulinaryBlog.Infrastructure.Auth;
using CulinaryBlog.Infrastructure.Observability;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Interceptors;
using CulinaryBlog.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "DefaultConnection";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        services.AddHttpContextAccessor();
        services.AddScoped<ICategoryValidator, DefaultCategoryValidator>();

        services.AddSingleton<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
            options.UseNpgsql(connectionString)
                .AddInterceptors(auditInterceptor);
        });
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddHostedService<DatabaseInitializer>();

        services.AddAuthInfrastructure(configuration);
        services.AddSingleton<ICacheInvalidator, NoOpCacheInvalidator>();

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton<IBackgroundJobService, HangfireBackgroundJobService>();

            services.AddHangfire(configuration => configuration
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(
                    options => options.UseNpgsqlConnection(connectionString),
                    new PostgreSqlStorageOptions
                    {
                        SchemaName = "hangfire",
                        PrepareSchemaIfNecessary = true
                    }));
            services.AddHangfireServer();

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
            {
                Attempts = 3,
                DelaysInSeconds = [60, 300, 1800]
            });
        }

        return services;
    }
}
