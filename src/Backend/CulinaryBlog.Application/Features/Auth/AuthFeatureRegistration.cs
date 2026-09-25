using CulinaryBlog.Application.Features.Auth.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Application.Features.Auth;

internal static class AuthFeatureRegistration
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services) =>
        services.AddScoped<AuthSessionIssuer>();
}
