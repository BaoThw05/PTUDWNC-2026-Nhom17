using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Register;

public sealed record RegisterCommand(string FullName, string Email, string UserName, string Password)
    : IRequest<AuthResponse>;
