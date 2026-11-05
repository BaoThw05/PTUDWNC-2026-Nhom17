using System.Net;
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
using Microsoft.IdentityModel.JsonWebTokens;

namespace CulinaryBlog.IntegrationTests.Auth;

/// <summary>Xoay khóa ký JWT: token ký bằng khóa cũ vẫn dùng được trong lúc chuyển, khóa đã gỡ thì bị từ chối.</summary>
public sealed class JwtKeyRotationTests : IClassFixture<ApiFactory>
{
    private const string ProbePath = "/api/v1/test-jwt-rotation";
    private const string NewSigningKey = "integration-tests-rotated-signing-key-9876543210";

    private readonly ApiFactory _oldKeyFactory;

    public JwtKeyRotationTests(ApiFactory oldKeyFactory) => _oldKeyFactory = oldKeyFactory;

    [Fact]
    public void IssuedToken_CarriesKeyId()
    {
        Assert.False(string.IsNullOrEmpty(new JsonWebToken(IssueToken(_oldKeyFactory)).Kid));
    }

    [Fact]
    public async Task TokenSignedWithPreviousKey_IsAccepted()
    {
        await using var rotated = new RotatedKeyFactory([ApiFactory.SigningKey]);

        var response = await SendAsync(rotated, IssueToken(_oldKeyFactory));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TokenSignedWithNewKey_IsAccepted()
    {
        await using var rotated = new RotatedKeyFactory([ApiFactory.SigningKey]);

        var response = await SendAsync(rotated, IssueToken(rotated));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TokenSignedWithRemovedKey_Returns401()
    {
        await using var rotated = new RotatedKeyFactory([]);

        var response = await SendAsync(rotated, IssueToken(_oldKeyFactory));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string IssueToken(WebApplicationFactory<Program> factory)
    {
        var user = new UserAccount(Guid.NewGuid(), "rotate@example.com", "rotate", "Rotate", null, true, DateTimeOffset.UtcNow, [Roles.Author]);
        return factory.Services.GetRequiredService<IAccessTokenIssuer>().Issue(user).Value;
    }

    private static async Task<HttpResponseMessage> SendAsync(WebApplicationFactory<Program> factory, string token)
    {
        using var client = factory.CreateClient();
        return await client.SendAsync(AuthApi.WithBearer(HttpMethod.Get, ProbePath, token));
    }

    private sealed class RotatedKeyFactory(string[] previousKeys) : ApiFactory
    {
        protected override IDictionary<string, string?> Settings
        {
            get
            {
                var settings = base.Settings;
                settings["Jwt:SigningKey"] = NewSigningKey;
                for (var i = 0; i < previousKeys.Length; i++)
                {
                    settings[$"Jwt:PreviousSigningKeys:{i}"] = previousKeys[i];
                }

                return settings;
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services => services.AddSingleton<IEndpointModule, ProbeEndpoints>());
        }
    }

    private sealed class ProbeEndpoints : IEndpointModule
    {
        public string Tag => "TestJwtRotation";

        public string Description => "Endpoint chỉ dùng trong integration test.";

        public void MapEndpoints(IEndpointRouteBuilder api) =>
            api.MapGet("/test-jwt-rotation", () => TypedResults.Ok()).RequireAuthorization();
    }
}
