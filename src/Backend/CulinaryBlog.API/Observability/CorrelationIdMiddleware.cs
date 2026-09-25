using Serilog.Context;

namespace CulinaryBlog.API.Observability;

internal sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value ?? "/"))
        using (LogContext.PushProperty("UserId", context.User.FindFirst("sub")?.Value ?? "anonymous"))
        {
            await next(context);
        }
    }
}
