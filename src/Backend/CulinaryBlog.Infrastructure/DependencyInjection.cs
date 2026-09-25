using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Infrastructure.Auth;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "DefaultConnection";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString(ConnectionStringName)));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddHostedService<DatabaseInitializer>();

        services.AddAuthInfrastructure(configuration);
        services.AddSingleton<ICacheInvalidator, NoOpCacheInvalidator>();

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
