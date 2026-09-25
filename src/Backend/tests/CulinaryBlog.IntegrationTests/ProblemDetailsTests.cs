using System.Net;
using System.Text.Json;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.IntegrationTests;

public sealed class ProblemDetailsTests : IClassFixture<IntegrationTestFactory>
{
    private const string ProblemJson = "application/problem+json";

    private readonly WebApplicationFactory<Program> _factory;

    public ProblemDetailsTests(IntegrationTestFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IEndpointModule, ThrowingEndpoints>()));
    }

    [Fact]
    public async Task Get_UnknownRoute_Returns404ProblemDetailsWithCode()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var body = await ReadProblemAsync(response);
        Assert.Equal("NOT_FOUND", body.RootElement.GetProperty("code").GetString());
        Assert.True(body.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task Get_ValidationFailure_Returns422WithErrors()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/test-errors/validation");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        using var body = await ReadProblemAsync(response);
        Assert.Equal("VALIDATION_ERROR", body.RootElement.GetProperty("code").GetString());
        Assert.Equal("Title is required.", body.RootElement.GetProperty("errors").GetProperty("Title")[0].GetString());
    }

    [Fact]
    public async Task Get_ConflictFailure_Returns409WithModuleCode()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/test-errors/conflict");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = await ReadProblemAsync(response);
        Assert.Equal("TEST_DUPLICATED", body.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_DbUpdateConcurrencyFailure_Returns409WithConcurrencyConflictCode()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/test-errors/concurrency");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = await ReadProblemAsync(response);
        Assert.Equal("CONCURRENCY_CONFLICT", body.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_UnhandledFailure_Returns500WithoutDetail()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/test-errors/unhandled");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        using var body = await ReadProblemAsync(response);
        Assert.Equal("INTERNAL_ERROR", body.RootElement.GetProperty("code").GetString());
        Assert.False(body.RootElement.TryGetProperty("detail", out _));
    }

    private static async Task<JsonDocument> ReadProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private sealed class ThrowingEndpoints : IEndpointModule
    {
        public string Tag => "TestErrors";

        public string Description => "Endpoint chỉ dùng trong integration test.";

        public void MapEndpoints(IEndpointRouteBuilder api)
        {
            var group = api.MapGroup("/test-errors");

            group.MapGet("/validation", IResult () => throw new ValidationException(
                new Dictionary<string, string[]> { ["Title"] = ["Title is required."] }));
            group.MapGet("/conflict", IResult () => throw new ConflictException("Duplicated.", "TEST_DUPLICATED"));
            group.MapGet("/concurrency", IResult () => throw new DbUpdateConcurrencyException("Concurrency conflict occurred."));
            group.MapGet("/unhandled", IResult () => throw new InvalidOperationException("Sensitive internals."));
        }
    }
}
