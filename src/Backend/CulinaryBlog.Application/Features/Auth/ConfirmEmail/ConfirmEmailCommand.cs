using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ConfirmEmail;

public sealed record ConfirmEmailCommand(Guid UserId, string Token) : IRequest;
