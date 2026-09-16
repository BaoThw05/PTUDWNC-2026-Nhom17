using System.Diagnostics;
using CulinaryBlog.Application.Common.Errors;

namespace CulinaryBlog.API.ErrorHandling;

internal static class ProblemDetailsExtensions
{
    public const string CodeKey = "code";
    public const string ErrorsKey = "errors";
    public const string TraceIdKey = "traceId";

    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var extensions = context.ProblemDetails.Extensions;

            // Lỗi không đi qua GlobalExceptionHandler (404 route, 401/403 của auth…) vẫn phải có code.
            extensions.TryAdd(CodeKey, CodeFromStatus(context.ProblemDetails.Status));
            extensions.TryAdd(TraceIdKey, Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    private static string CodeFromStatus(int? status) => status switch
    {
        StatusCodes.Status401Unauthorized => ErrorCodes.Unauthorized,
        StatusCodes.Status403Forbidden => ErrorCodes.Forbidden,
        StatusCodes.Status404NotFound => ErrorCodes.NotFound,
        StatusCodes.Status409Conflict => ErrorCodes.Conflict,
        StatusCodes.Status422UnprocessableEntity => ErrorCodes.ValidationError,
        StatusCodes.Status429TooManyRequests => ErrorCodes.TooManyRequests,
        >= StatusCodes.Status500InternalServerError => ErrorCodes.InternalError,
        _ => ErrorCodes.BadRequest,
    };
}
