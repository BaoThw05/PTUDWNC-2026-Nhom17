using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.AccountEmail;
using CulinaryBlog.Application.Features.Auth.Common;
using CulinaryBlog.Application.Features.Auth.LoginHistory;
using CulinaryBlog.Application.Features.Auth.PasswordResetEmail;
using CulinaryBlog.Application.Features.Auth.RefreshTokenCleanup;
using CulinaryBlog.Application.Features.Auth.WelcomeEmail;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Application.Features.Auth;

internal static class AuthFeatureRegistration
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services) =>
        services
            .AddScoped<AuthSessionIssuer>()
            .AddScoped<LoginAudit>()
            .AddScoped<LoginAttemptCleanupJob>()
            .AddScoped<IWelcomeEmailJob, WelcomeEmailJob>()
            .AddScoped<IPasswordResetEmailJob, PasswordResetEmailJob>()
            .AddScoped<IAccountEmailJob, AccountEmailJob>()
            .AddScoped<RefreshTokenCleanupJob>();
}
