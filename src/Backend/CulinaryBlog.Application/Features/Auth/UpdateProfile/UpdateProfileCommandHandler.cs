using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.UpdateProfile;

public sealed class UpdateProfileCommandHandler(ICurrentUser currentUser, IUserAccountService users)
    : IRequestHandler<UpdateProfileCommand, UserProfileResponse>
{
    public async Task<UserProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("Authentication is required.");

        var user = await users.UpdateDisplayNameAsync(userId, request.DisplayName.Trim(), cancellationToken);

        return UserProfileResponse.From(user);
    }
}
