using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.AdminUsers;

/// <summary>Admin khóa/mở tài khoản (<c>IsActive</c>) và gán vai trò; trường null thì giữ nguyên.</summary>
public sealed record UpdateUserCommand(Guid UserId, bool? IsActive, IReadOnlyList<string>? Roles) : IRequest<AdminUserResponse>;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command)
            .Must(command => command.IsActive is not null || command.Roles is not null)
            .WithName("Body")
            .WithMessage("Provide isActive or roles.");

        RuleFor(command => command.Roles!)
            .NotEmpty()
            .Must(roles => roles.Distinct().Count() == roles.Count).WithMessage("Roles must not repeat.")
            .ForEach(role => role.Must(Roles.All.Contains).WithMessage("Unknown role '{PropertyValue}'."))
            .When(command => command.Roles is not null);
    }
}

/// <summary>
/// Khóa tài khoản thì thu hồi mọi refresh token: người dùng không làm mới phiên được nữa.
/// Admin không được tự khóa mình hoặc tự bỏ vai trò Admin, nên hệ thống luôn còn ít nhất một Admin.
/// </summary>
public sealed class UpdateUserCommandHandler(
    ICurrentUser currentUser,
    IUserAccountService users,
    IRefreshTokenRepository refreshTokens,
    TimeProvider timeProvider) : IRequestHandler<UpdateUserCommand, AdminUserResponse>
{
    public async Task<AdminUserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == currentUser.UserId
            && (request.IsActive == false || request.Roles is { } roles && !roles.Contains(Domain.Auth.Roles.Admin)))
        {
            throw new ForbiddenException("Admins cannot disable or demote themselves.", AuthErrorCodes.AdminSelfLockout);
        }

        var user = await users.UpdateAccessAsync(request.UserId, request.IsActive, request.Roles, cancellationToken);

        if (request.IsActive == false)
        {
            // ponytail: access token đang cầm vẫn dùng được tới khi hết hạn (≤ 15 phút); cần chặn ngay thì kiểm IsActive mỗi request.
            var now = timeProvider.GetUtcNow();
            foreach (var token in await refreshTokens.GetActiveForUserAsync(user.Id, now, cancellationToken))
            {
                token.Revoke(RefreshTokenRevokeReason.AccountDisabled, now);
            }

            await refreshTokens.TrySaveChangesAsync(cancellationToken);
        }

        return AdminUserResponse.From(user);
    }
}
