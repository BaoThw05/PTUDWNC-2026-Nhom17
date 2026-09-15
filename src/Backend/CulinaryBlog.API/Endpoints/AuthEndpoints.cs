using System.Security.Claims;
using CulinaryBlog.Application.Auth.GetCurrentUser;
using CulinaryBlog.Application.Auth.GoogleLogin;
using CulinaryBlog.Application.Auth.Login;
using CulinaryBlog.Application.Auth.Logout;
using CulinaryBlog.Application.Auth.RefreshTokens;
using CulinaryBlog.Application.Auth.Register;
using CulinaryBlog.Application.Auth.UpdateProfile;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public record RegisterRequest(string FullName, string Email, string UserName, string Password);
public record LoginRequest(string Email, string Password);
public record GoogleLoginRequest(string IdToken);
public record RefreshTokenRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);
public record UpdateProfileRequest(string? FullName, string? AvatarUrl);

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth")
            .RequireRateLimiting("auth");

        group.MapPost("/register", async (RegisterRequest request, ISender sender, HttpContext http, CancellationToken ct) =>
        {
            var ip = GetClientIp(http);
            var result = await sender.Send(
                new RegisterCommand(request.FullName, request.Email, request.UserName, request.Password, ip), ct);
            return Results.Created("/api/v1/auth/me", result);
        });

        group.MapPost("/login", async (LoginRequest request, ISender sender, HttpContext http, CancellationToken ct) =>
        {
            var ip = GetClientIp(http);
            var result = await sender.Send(new LoginCommand(request.Email, request.Password, ip), ct);
            return Results.Ok(result);
        });

        group.MapPost("/google", async (GoogleLoginRequest request, ISender sender, HttpContext http, CancellationToken ct) =>
        {
            var ip = GetClientIp(http);
            var result = await sender.Send(new GoogleLoginCommand(request.IdToken, ip), ct);
            return Results.Ok(result);
        });

        group.MapPost("/refresh", async (RefreshTokenRequest request, ISender sender, HttpContext http, CancellationToken ct) =>
        {
            var ip = GetClientIp(http);
            var result = await sender.Send(new RefreshTokenCommand(request.RefreshToken, ip), ct);
            return Results.Ok(result);
        });

        group.MapPost("/logout", async (LogoutRequest request, ISender sender, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            await sender.Send(new LogoutCommand(request.RefreshToken, userId), ct);
            return Results.NoContent();
        }).RequireAuthorization();

        group.MapGet("/me", async (ISender sender, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            var result = await sender.Send(new GetCurrentUserQuery(userId), ct);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPatch("/me", async (UpdateProfileRequest request, ISender sender, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            var result = await sender.Send(new UpdateProfileCommand(userId, request.FullName, request.AvatarUrl), ct);
            return Results.Ok(result);
        }).RequireAuthorization();

        return app;
    }

    private static string GetUserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Authenticated request is missing NameIdentifier claim.");

    private static string? GetClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString();
}
