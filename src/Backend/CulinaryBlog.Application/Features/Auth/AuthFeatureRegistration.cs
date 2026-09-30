using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using CulinaryBlog.Application.Features.Auth.WelcomeEmail;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Application.Features.Auth;

internal static class AuthFeatureRegistration
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services) =>
        services
            .AddScoped<AuthSessionIssuer>()
            .AddScoped<IWelcomeEmailJob, WelcomeEmailJob>();
}
