using CulinaryBlog.Application.Abstractions;
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
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICategoryValidator, DefaultCategoryValidator>();

        services.AddSingleton<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
            options.UseNpgsql(configuration.GetConnectionString(ConnectionStringName))
                .AddInterceptors(auditInterceptor);
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
