using CulinaryBlog.Application.Common.Errors;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.API.ErrorHandling;

/// <summary>
/// Chuyển exception thành Problem Details RFC 9457 kèm <c>code</c> (S-10c).
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, code) = Map(exception);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            logger.LogInformation("Request failed with {StatusCode} {ErrorCode}", status, code);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            // Không lộ chi tiết lỗi hệ thống ra ngoài.
            Detail = status >= StatusCodes.Status500InternalServerError ? null : exception.Message,
        };
        problem.Extensions[ProblemDetailsExtensions.CodeKey] = code;

        if (exception is ValidationException { Errors.Count: > 0 } validation)
        {
            problem.Extensions[ProblemDetailsExtensions.ErrorsKey] = validation.Errors;
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    // Module có exception riêng (ví dụ 423 tài khoản bị khóa) thì thêm nhánh ở đây — xem mục file dùng chung trong docs/OWNERSHIP.md.
    private static (int Status, string Code) Map(Exception exception) => exception switch
    {
        ValidationException e => (StatusCodes.Status422UnprocessableEntity, e.Code),
        NotFoundException e => (StatusCodes.Status404NotFound, e.Code),
        ConflictException e => (StatusCodes.Status409Conflict, e.Code),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, ErrorCodes.ConcurrencyConflict),
        ForbiddenException e => (StatusCodes.Status403Forbidden, e.Code),
        AppException e => (StatusCodes.Status422UnprocessableEntity, e.Code),
        BadHttpRequestException e => (e.StatusCode, ErrorCodes.BadRequest),
        _ => (StatusCodes.Status500InternalServerError, ErrorCodes.InternalError),
    };
}
