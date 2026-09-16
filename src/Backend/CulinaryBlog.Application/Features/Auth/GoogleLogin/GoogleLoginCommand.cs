using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.GoogleLogin;

/// <param name="IdToken">id_token Google mà Auth.js nhận được; backend tự xác minh (S-05).</param>
public sealed record GoogleLoginCommand(string IdToken) : IRequest<AuthResponse>;
