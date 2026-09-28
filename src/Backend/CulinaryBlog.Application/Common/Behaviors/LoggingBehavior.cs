using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CulinaryBlog.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                logger.LogWarning(
                    "Slow request {RequestName} completed in {ElapsedMilliseconds:F0} ms",
                    requestName,
                    elapsedMilliseconds);
            }
            else
            {
                logger.LogInformation(
                    "Request {RequestName} completed in {ElapsedMilliseconds:F0} ms",
                    requestName,
                    elapsedMilliseconds);
            }
        }
    }
}
