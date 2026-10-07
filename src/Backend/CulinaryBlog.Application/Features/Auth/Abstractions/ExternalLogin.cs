namespace CulinaryBlog.Application.Features.Auth.Abstractions;

public sealed record ExternalLogin(string Provider, string ProviderKey);
