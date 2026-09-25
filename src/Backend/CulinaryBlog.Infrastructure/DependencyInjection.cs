using Amazon.Runtime;
using Amazon.S3;
using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Storage;
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
        AddFileStorage(services, configuration);

        return services;
    }

    private static void AddFileStorage(IServiceCollection services, IConfiguration configuration)
    {
        var options = new FileStorageOptions
        {
            Provider = configuration["FileStorage:Provider"] ?? "Local",
            PublicBaseUrl = configuration["FileStorage:PublicBaseUrl"] ?? "/media",
            LocalRootPath = configuration["FileStorage:LocalRootPath"] ?? "storage",
            S3 = new S3StorageOptions
            {
                ServiceUrl = configuration["FileStorage:S3:ServiceUrl"] ?? string.Empty,
                AccessKey = configuration["FileStorage:S3:AccessKey"] ?? string.Empty,
                SecretKey = configuration["FileStorage:S3:SecretKey"] ?? string.Empty,
                BucketName = configuration["FileStorage:S3:BucketName"] ?? string.Empty,
            },
        };

        services.AddSingleton(options);

        if (string.Equals(options.Provider, "S3", StringComparison.OrdinalIgnoreCase))
        {
            ValidateS3Options(options.S3);
            services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
                new BasicAWSCredentials(options.S3.AccessKey, options.S3.SecretKey),
                new AmazonS3Config { ServiceURL = options.S3.ServiceUrl, ForcePathStyle = true }));
            services.AddSingleton<IFileStorage, S3FileStorage>();
            return;
        }

        if (string.Equals(options.Provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IFileStorage, LocalFileStorage>();
            return;
        }

        throw new InvalidOperationException($"FileStorage provider không được hỗ trợ: {options.Provider}.");
    }

    private static void ValidateS3Options(S3StorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceUrl)
            || string.IsNullOrWhiteSpace(options.AccessKey)
            || string.IsNullOrWhiteSpace(options.SecretKey)
            || string.IsNullOrWhiteSpace(options.BucketName))
        {
            throw new InvalidOperationException("Cấu hình FileStorage:S3 chưa đầy đủ.");
        }
    }
}
