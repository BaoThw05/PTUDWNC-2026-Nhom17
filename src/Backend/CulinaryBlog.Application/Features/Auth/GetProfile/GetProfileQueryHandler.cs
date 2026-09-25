using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.GetProfile;

public sealed class GetProfileQueryHandler(ICurrentUser currentUser, IUserAccountService users)
    : IRequestHandler<GetProfileQuery, UserProfileResponse>
{
    public async Task<UserProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("Authentication is required.");

        var user = await users.FindByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("The user no longer exists.", AuthErrorCodes.UserNotFound);

        return UserProfileResponse.From(user);
    }
}
