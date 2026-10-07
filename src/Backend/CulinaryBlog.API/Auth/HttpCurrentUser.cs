using CulinaryBlog.Application.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CulinaryBlog.API.Auth;

internal sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private HttpContext? Context => httpContextAccessor.HttpContext;

    public Guid? UserId =>
        Guid.TryParse(Context?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId) ? userId : null;

    public string? IpAddress => Context?.Connection.RemoteIpAddress?.ToString();

    public bool IsInRole(string role) => Context?.User.IsInRole(role) ?? false;
}
