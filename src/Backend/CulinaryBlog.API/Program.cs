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

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddApiProblemDetails();
builder.Services.AddApiOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointModules();
builder.Services.AddAuthApi(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.Services.GetRequiredService<IBackgroundJobService>().AddOrUpdateRecurring<IRecipeImageOrphanCleanupJob>(
        "recipe-image-orphan-cleanup", job => job.ExecuteAsync(), "0 3 * * *");
}

// Ghi log request ở ngoài cùng để log đúng status sau khi exception đã được đổi thành Problem Details.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();

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
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapEndpointModules();

app.Run();
