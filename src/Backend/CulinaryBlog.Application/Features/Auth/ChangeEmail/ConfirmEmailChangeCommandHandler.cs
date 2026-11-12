using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ChangeEmail;

public sealed class ConfirmEmailChangeCommandHandler(ICurrentUser currentUser, IUserAccountService users)
    : IRequestHandler<ConfirmEmailChangeCommand, UserProfileResponse>
{
    public async Task<UserProfileResponse> Handle(ConfirmEmailChangeCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("Authentication is required.");
        var user = await users.ChangeEmailAsync(userId, request.NewEmail.Trim(), request.Code, cancellationToken);
        return UserProfileResponse.From(user);
    }
}
