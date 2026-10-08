using IPNetwork = System.Net.IPNetwork;
using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.API.Auth;

internal static class AuthApiRegistration
{
    private static readonly TimeSpan ClockSkew = TimeSpan.FromSeconds(30);

    public static IServiceCollection AddAuthApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>(ConfigureJwtBearer);

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.Author, policy => policy.RequireRole(Roles.Author, Roles.Admin))
            .AddPolicy(AuthPolicies.Admin, policy => policy.RequireRole(Roles.Admin));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        var rateLimitSection = configuration.GetSection(AuthRateLimitOptions.SectionName);
        services.Configure<AuthRateLimitOptions>(rateLimitSection);
        services.AddRateLimiter(AuthRateLimitPolicies.Configure);

        var trustedNetworks = rateLimitSection.Get<AuthRateLimitOptions>()?.TrustedProxyNetworks ?? [];
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            foreach (var network in trustedNetworks)
            {
                options.KnownIPNetworks.Add(IPNetwork.Parse(network));
            }
        });

        return services;
    }

    /// <summary>Thứ tự: IP thật từ proxy → xác thực → rate limit (cần biết user) → phân quyền.</summary>
    public static IApplicationBuilder UseAuthApi(this IApplicationBuilder app) =>
        app.UseForwardedHeaders()
            .UseAuthentication()
            .UseRateLimiter()
            .UseAuthorization();

    private static void ConfigureJwtBearer(JwtBearerOptions bearer, IOptions<JwtOptions> jwtOptions)
    {
        var jwt = jwtOptions.Value;

        // Giữ nguyên tên claim trong token (sub, role…) thay vì đổi sang tên dài kiểu WS-Federation.
        bearer.MapInboundClaims = false;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = jwt.CreateSigningKey(),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = JwtOptions.NameClaimType,
            RoleClaimType = JwtOptions.RoleClaimType,
            ClockSkew = ClockSkew,
        };
    }
}
