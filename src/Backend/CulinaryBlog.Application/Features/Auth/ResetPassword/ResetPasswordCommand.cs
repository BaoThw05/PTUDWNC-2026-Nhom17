using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ResetPassword;

public sealed record ResetPasswordCommand(Guid UserId, string Token, string NewPassword) : IRequest;
