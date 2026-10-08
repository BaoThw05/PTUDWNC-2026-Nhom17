namespace CulinaryBlog.Application.Features.Auth.Abstractions;

public sealed record GoogleIdentity(string Subject, string Email, bool EmailVerified, string? Name, string? PictureUrl);
