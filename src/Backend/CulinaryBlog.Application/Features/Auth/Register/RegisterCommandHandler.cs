using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler(IUserAccountService users, AuthSessionIssuer sessions)
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

        // TODO(TV1): IWelcomeEmailJob đã sẵn sàng; chỉ còn thiếu IBackgroundJobService (TV3, 3.07) để enqueue thay vì gọi trực tiếp (việc 1.15).
        return await sessions.StartAsync(user, cancellationToken);
    }
}
