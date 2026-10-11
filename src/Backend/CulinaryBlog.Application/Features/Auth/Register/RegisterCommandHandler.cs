using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler(
    IUserAccountService users,
    AuthSessionIssuer sessions,
    IBackgroundJobService jobs)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        if (await users.FindByEmailAsync(email, cancellationToken) is not null)
        {
            throw new ConflictException("The email is already registered.", AuthErrorCodes.EmailExists);
        }

        var user = await users.CreateAsync(
            new NewUserAccount(email, request.FullName.Trim(), request.UserName.Trim(), request.Password),
            cancellationToken);

        var response = await sessions.StartAsync(user, cancellationToken);

        // FR-JOB-001: enqueue sau khi user và refresh token đã lưu; lỗi gửi mail thì Hangfire tự retry.
        jobs.Enqueue<IWelcomeEmailJob>(job => job.ExecuteAsync(user.Id, CancellationToken.None));
        return response;
    }
}
