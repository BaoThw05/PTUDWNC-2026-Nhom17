using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, code, title, errors) = exception switch
        {
            AppException appEx => (appEx.StatusCode, appEx.Code, appEx.Message, (object?)null),

            FluentValidation.ValidationException validationEx => (
                StatusCodes.Status422UnprocessableEntity,
                "VALIDATION_ERROR",
                "Dữ liệu đầu vào không hợp lệ",
                (object?)validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "RECIPE_CONCURRENCY_CONFLICT",
                "Dữ liệu đã bị người khác thay đổi, vui lòng tải lại",
                (object?)null),

            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", "Đã có lỗi hệ thống xảy ra", (object?)null)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://culinaryblog.dev/errors/{code.ToLowerInvariant().Replace('_', '-')}",
            Extensions =
            {
                ["code"] = code,
                ["traceId"] = httpContext.TraceIdentifier,
                ["errors"] = errors
            }
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}