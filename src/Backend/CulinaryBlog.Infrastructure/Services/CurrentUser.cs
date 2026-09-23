using System.Security.Claims;
using CulinaryBlog.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CulinaryBlog.Infrastructure.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;
    private ClaimsPrincipal? User => HttpContext?.User;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? (HttpContext?.Request.Headers.TryGetValue("X-User-Id", out var userId) == true && !string.IsNullOrWhiteSpace(userId)
            ? userId.ToString()
            : null);

    public string? Role =>
        User?.FindFirstValue(ClaimTypes.Role)
        ?? (HttpContext?.Request.Headers.TryGetValue("X-User-Role", out var role) == true && !string.IsNullOrWhiteSpace(role)
            ? role.ToString()
            : null);

    public bool IsAuthenticated => (User?.Identity?.IsAuthenticated ?? false) || !string.IsNullOrWhiteSpace(UserId);

    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase) || (User?.IsInRole("Admin") ?? false);
}
