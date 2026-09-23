using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Infrastructure.Persistence;
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

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
