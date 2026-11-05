using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ConfirmEmail;

public sealed record ResendEmailConfirmationCommand : IRequest;

/// <summary>Email đã xác nhận thì không gửi gì, vẫn trả thành công.</summary>
public sealed class ResendEmailConfirmationCommandHandler(
    ICurrentUser currentUser,
    IUserAccountService users,
    IBackgroundJobService jobs) : IRequestHandler<ResendEmailConfirmationCommand>
{
    public async Task Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("Authentication is required.");
        var user = await users.FindByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("The user no longer exists.", AuthErrorCodes.UserNotFound);

        if (!user.EmailConfirmed)
        {
            jobs.Enqueue<IAccountEmailJob>(job => job.SendConfirmationAsync(userId, CancellationToken.None));
        }
    }
}
