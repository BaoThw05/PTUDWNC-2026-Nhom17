using System.Net;
using CulinaryBlog.API.Auth;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.IntegrationTests.Auth;

/// <summary>Việc 1.07: policy dùng chung trả 401/403/200 đúng.</summary>
public sealed class AuthorizationPolicyTests : IClassFixture<ApiFactory>
{
    private const string AuthorPath = "/api/v1/test-policies/author";
    private const string AdminPath = "/api/v1/test-policies/admin";

    private readonly WebApplicationFactory<Program> _factory;

    public AuthorizationPolicyTests(ApiFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IEndpointModule, PolicyProbeEndpoints>()));
    }

    [Fact]
    public async Task AuthorPolicy_WithoutToken_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(AuthorPath);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthorPolicy_WithAuthorToken_Returns200()
    {
        var response = await SendAsync(AuthorPath, Roles.Author);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminPolicy_WithAuthorToken_Returns403()
    {
        var response = await SendAsync(AdminPath, Roles.Author);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("FORBIDDEN", await AuthApi.ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task AdminPolicy_WithAdminToken_Returns200()
    {
        var response = await SendAsync(AdminPath, Roles.Admin);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthorPolicy_WithTamperedToken_Returns401()
    {
        using var client = _factory.CreateClient();
        var token = IssueToken(Roles.Author) + "x";

        var response = await client.SendAsync(AuthApi.WithBearer(HttpMethod.Get, AuthorPath, token));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendAsync(string path, string role)
    {
        using var client = _factory.CreateClient();
        return await client.SendAsync(AuthApi.WithBearer(HttpMethod.Get, path, IssueToken(role)));
    }

    private string IssueToken(string role)
    {
        var issuer = _factory.Services.GetRequiredService<IAccessTokenIssuer>();
        var user = new UserAccount(Guid.NewGuid(), "probe@example.com", "Probe", null, true, DateTimeOffset.UtcNow, [role]);
        return issuer.Issue(user).Value;
    }

    private sealed class PolicyProbeEndpoints : IEndpointModule
    {
        public string Tag => "TestPolicies";

        public string Description => "Endpoint chỉ dùng trong integration test.";

        public void MapEndpoints(IEndpointRouteBuilder api)
        {
            var group = api.MapGroup("/test-policies");
            group.MapGet("/author", () => TypedResults.Ok()).RequireAuthorization(AuthPolicies.Author);
            group.MapGet("/admin", () => TypedResults.Ok()).RequireAuthorization(AuthPolicies.Admin);
        }
    }
}
