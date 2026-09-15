using CulinaryBlog.Application.Auth.Dtos;
using MediatR;

namespace CulinaryBlog.Application.Auth.GoogleLogin;

public record GoogleLoginCommand(string IdToken, string? IpAddress) : IRequest<AuthResponseDto>;
