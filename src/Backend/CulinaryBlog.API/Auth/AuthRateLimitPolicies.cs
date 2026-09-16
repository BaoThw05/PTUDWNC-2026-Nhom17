using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CulinaryBlog.API.Auth;

internal static class AuthRateLimitPolicies
{
    public const string Credentials = "auth-credentials";
    public const string Refresh = "auth-refresh";

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public static void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = WriteRejectionAsync;

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            FixedWindow(UserOrIpKey(context), Limits(context).GeneralPermitLimit));

        options.AddPolicy(Credentials, context =>
            FixedWindow($"credentials:{IpOf(context)}", Limits(context).CredentialsPermitLimit));

        options.AddPolicy(Refresh, context =>
            FixedWindow($"refresh:{IpOf(context)}", Limits(context).RefreshPermitLimit));
    }

    private static RateLimitPartition<string> FixedWindow(string key, int permitLimit) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = Window,
            QueueLimit = 0,
        });

    private static AuthRateLimitOptions Limits(HttpContext context) =>
        context.RequestServices.GetRequiredService<IOptions<AuthRateLimitOptions>>().Value;

    private static string UserOrIpKey(HttpContext context) =>
        context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value is { } userId
            ? $"user:{userId}"
            : $"ip:{IpOf(context)}";

    private static string IpOf(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static async ValueTask WriteRejectionAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        var problemDetails = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = { Status = StatusCodes.Status429TooManyRequests, Detail = "Too many requests." },
        });
    }
}
