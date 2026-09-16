namespace CulinaryBlog.Application.Features.Auth.Abstractions;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
