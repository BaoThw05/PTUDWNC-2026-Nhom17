using CulinaryBlog.API.Auth;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.ErrorHandling;
using CulinaryBlog.API.OpenApi;
using CulinaryBlog.Application;
using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Infrastructure;
using Hangfire;
using CulinaryBlog.API.Observability;
using Scalar.AspNetCore;
using Serilog;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using CulinaryBlog.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddApiProblemDetails();
builder.Services.AddApiOpenApi();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);
builder.Services.AddEndpointModules();
builder.Services.AddAuthApi(builder.Configuration);

var redisConnection = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("public-recipes", policy => policy
        .Expire(TimeSpan.FromMinutes(5))
        .SetVaryByQuery("*")
        .Tag("recipes"));
    options.AddPolicy("public-recipe-search", policy => policy
        .Expire(TimeSpan.FromMinutes(1))
        .SetVaryByQuery("*")
        .Tag("recipes"));
    options.AddPolicy("public-categories", policy => policy
        .Expire(TimeSpan.FromMinutes(30))
        .SetVaryByQuery("*")
        .Tag("categories"));
});
if (!string.IsNullOrWhiteSpace(redisConnection))
    builder.Services.AddStackExchangeRedisOutputCache(options => options.Configuration = redisConnection);
builder.Services.AddSingleton<ICacheInvalidator, ApiOutputCacheInvalidator>();

var app = builder.Build();

// Ghi log request ở ngoài cùng để log đúng status sau khi exception đã được đổi thành Problem Details.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHangfireDashboard("/hangfire");
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthApi();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});
app.MapEndpointModules();

app.Run();

internal sealed class ApiOutputCacheInvalidator(IOutputCacheStore outputCacheStore) : ICacheInvalidator
{
    public async Task InvalidateAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        foreach (var tag in tags.Distinct(StringComparer.Ordinal))
            await outputCacheStore.EvictByTagAsync(tag, cancellationToken);
    }
}

internal sealed class DatabaseHealthCheck(AppDbContext dbContext) : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database is available.")
            : Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Database is unavailable.");
    }
}
