using System.Net;
using System.Text.Json;
using CulinaryBlog.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title) = exception switch
        {
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            NotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            AppUnauthorizedException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            AccountLockedException => (HttpStatusCode.Locked, "Account Locked"),
            ValidationException => (HttpStatusCode.UnprocessableEntity, "Validation Error"),
            BadRequestException => (HttpStatusCode.BadRequest, "Bad Request"),
            ExternalServiceException => (HttpStatusCode.BadGateway, "External Service Error"),
            _ => (HttpStatusCode.InternalServerError, "Server Error"),
        };

        context.Response.StatusCode = (int)statusCode;

        object problemDetails = exception is ValidationException validationException
            ? new ValidationProblemDetails(
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = context.Response.StatusCode,
                Title = title,
                Instance = context.Request.Path,
            }
            : new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = title,
                Detail = statusCode == HttpStatusCode.InternalServerError
                    ? "An unexpected error occurred. Please try again later."
                    : exception.Message,
                Instance = context.Request.Path,
            };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }
}
