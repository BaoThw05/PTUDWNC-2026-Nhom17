using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.IntegrationTests.Auth;

/// <summary>Việc 1.19: vượt hạn mức trả 429 kèm Retry-After.</summary>
public sealed class RateLimitingTests(RateLimitingTests.LowLimitFactory factory)
    : IClassFixture<RateLimitingTests.LowLimitFactory>
{
    private const int CredentialsLimit = 2;

    [Fact]
    public async Task Login_OverLimit_Returns429WithRetryAfter()
    {
        using var client = factory.CreateClient();

        // Body sai để request dừng ở validation (422), không cần database; rate limit vẫn tính.
        for (var attempt = 0; attempt < CredentialsLimit; attempt++)
        {
            var allowed = await AuthApi.LoginAsync(client, string.Empty, string.Empty);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, allowed.StatusCode);
        }

        var rejected = await AuthApi.LoginAsync(client, string.Empty, string.Empty);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.NotNull(rejected.Headers.RetryAfter);
        Assert.Equal("TOO_MANY_REQUESTS", await AuthApi.ReadErrorCodeAsync(rejected));
    }

    [Fact]
    public async Task Login_OverLimitFromOneIp_DoesNotBlockAnotherIp()
    {
        using var client = factory.CreateClient();

        for (var attempt = 0; attempt < CredentialsLimit; attempt++)
        {
            await LoginFromAsync(client, "10.0.0.1");
        }

        var blocked = await LoginFromAsync(client, "10.0.0.1");
        var otherIp = await LoginFromAsync(client, "10.0.0.2");

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, otherIp.StatusCode);
    }

    private static Task<HttpResponseMessage> LoginFromAsync(HttpClient client, string ip)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{AuthApi.BasePath}/login")
        {
            Content = JsonContent.Create(new { email = string.Empty, password = string.Empty }),
        };
        request.Headers.Add(LowLimitFactory.ClientIpHeader, ip);
        return client.SendAsync(request);
    }

    public sealed class LowLimitFactory : ApiFactory
    {
        public const string ClientIpHeader = "X-Test-Client-IP";

        protected override IDictionary<string, string?> Settings
        {
            get
            {
                var settings = base.Settings;
                settings["RateLimiting:CredentialsPermitLimit"] = CredentialsLimit.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return settings;
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services => services.AddSingleton<IStartupFilter, ClientIpStartupFilter>());
        }

        // TestServer không có IP client; đặt IP giả từ header để mỗi request rơi vào đúng phân vùng rate limit.
        private sealed class ClientIpStartupFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
            {
                app.Use((context, nextMiddleware) =>
                {
                    if (IPAddress.TryParse(context.Request.Headers[ClientIpHeader].ToString(), out var ip))
                    {
                        context.Connection.RemoteIpAddress = ip;
                    }

                    return nextMiddleware(context);
                });
                next(app);
            };
        }
    }
}
