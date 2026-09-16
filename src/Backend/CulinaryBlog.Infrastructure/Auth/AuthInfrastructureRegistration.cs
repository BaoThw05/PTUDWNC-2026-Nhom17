using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Auth;

internal static class AuthInfrastructureRegistration
{
    private const int MaxFailedAccessAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentityCore<ApplicationUser>(ConfigureIdentity)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                jwt => System.Text.Encoding.UTF8.GetByteCount(jwt.SigningKey) >= JwtOptions.MinSigningKeyBytes,
                $"Jwt:SigningKey must be at least {JwtOptions.MinSigningKeyBytes} bytes.")
            .ValidateOnStart();
        services.AddSingleton<IPostConfigureOptions<JwtOptions>, JwtDevelopmentKeySetup>();

        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));
        services.Configure<AuthSeedOptions>(configuration.GetSection(AuthSeedOptions.SectionName));

        services.AddScoped<IUserAccountService, IdentityUserAccountService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddSingleton<IGoogleIdTokenValidator, GoogleIdTokenValidator>();
        services.AddScoped<IDataSeeder, AuthDataSeeder>();

        return services;
    }

    // NFR-SEC-001 và FR-AUTH-002: mật khẩu mạnh, khóa 15 phút sau 5 lần sai.
    private static void ConfigureIdentity(IdentityOptions options)
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = MaxFailedAccessAttempts;
        options.Lockout.DefaultLockoutTimeSpan = LockoutDuration;

        options.User.RequireUniqueEmail = true;
        // UserName = email; email đã được validator kiểm tra nên không giới hạn ký tự.
        options.User.AllowedUserNameCharacters = string.Empty;
    }
}
