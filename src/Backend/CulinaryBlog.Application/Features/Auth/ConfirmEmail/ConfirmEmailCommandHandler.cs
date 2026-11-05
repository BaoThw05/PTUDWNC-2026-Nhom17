using CulinaryBlog.Application.Features.Auth.Abstractions;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler(IUserAccountService users) : IRequestHandler<ConfirmEmailCommand>
{
    public Task Handle(ConfirmEmailCommand request, CancellationToken cancellationToken) =>
        users.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
}
