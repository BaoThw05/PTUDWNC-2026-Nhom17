using System.Net;
using CulinaryBlog.IntegrationTests.Infrastructure;

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

    public sealed class LowLimitFactory : ApiFactory
    {
        protected override IDictionary<string, string?> Settings
        {
            get
            {
                var settings = base.Settings;
                settings["RateLimiting:CredentialsPermitLimit"] = CredentialsLimit.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return settings;
            }
        }
    }
}
