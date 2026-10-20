using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ForgotPassword;

/// <summary>Luôn thành công dù email có tồn tại hay không, để không lộ tài khoản nào đã đăng ký.</summary>
public sealed class ForgotPasswordCommandHandler(IUserAccountService users, IBackgroundJobService jobs)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is not { IsActive: true })
        {
            return;
        }

        jobs.Enqueue<IPasswordResetEmailJob>(job => job.ExecuteAsync(user.Id, CancellationToken.None));
    }
}
