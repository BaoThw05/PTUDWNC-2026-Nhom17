using System.Net;
using CulinaryBlog.IntegrationTests.Infrastructure;

namespace CulinaryBlog.IntegrationTests;

public sealed class HealthEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task GetHealth_WhenApiRunning_Returns200()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
