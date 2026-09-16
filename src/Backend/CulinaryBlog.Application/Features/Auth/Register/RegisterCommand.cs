using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Register;

public sealed record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<AuthResponse>;
