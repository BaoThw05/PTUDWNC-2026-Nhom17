using CulinaryBlog.API.Auth;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.ErrorHandling;
using CulinaryBlog.API.OpenApi;
using CulinaryBlog.Application;
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

// Ghi log request ở ngoài cùng để log đúng status sau khi exception đã được đổi thành Problem Details.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new AllowAllDashboardAuthorizationFilter()]
    });
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

// Task 2.21: Lập lịch recurring job dọn dẹp thùng rác công thức hằng ngày (S-03)
var backgroundJobs = app.Services.GetService<CulinaryBlog.Application.Abstractions.IBackgroundJobService>();
if (backgroundJobs is not null)
{
    backgroundJobs.AddOrUpdateRecurring<CulinaryBlog.Application.Features.Recipes.RecipeTrashCleanupJob>(
        "recipe-trash-cleanup",
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Daily());
}

app.Run();

sealed class AllowAllDashboardAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context) => true;
}
