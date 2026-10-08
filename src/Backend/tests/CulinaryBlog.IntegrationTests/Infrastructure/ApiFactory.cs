using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CulinaryBlog.IntegrationTests.Infrastructure;

/// <summary>
/// Chạy API ở môi trường "Testing": không đọc appsettings.Development.json và user-secrets,
/// nên test không bao giờ chạm vào database dev trên máy.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string SigningKey = "integration-tests-signing-key-0123456789abcdef";

    protected virtual IDictionary<string, string?> Settings => new Dictionary<string, string?>
    {
        ["Jwt:SigningKey"] = SigningKey,
        ["RateLimiting:CredentialsPermitLimit"] = "10000",
        ["RateLimiting:RefreshPermitLimit"] = "10000",
        ["RateLimiting:GeneralPermitLimit"] = "10000",
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        foreach (var (key, value) in Settings)
        {
            builder.UseSetting(key, value);
        }
    }
}
