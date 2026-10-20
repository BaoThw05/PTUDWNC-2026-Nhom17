namespace CulinaryBlog.Application.Features.Auth.Abstractions;

/// <summary>Hợp đồng job nền gửi email đặt lại mật khẩu; enqueue qua <c>IBackgroundJobService</c> khi người dùng quên mật khẩu.</summary>
public interface IPasswordResetEmailJob
{
    Task ExecuteAsync(Guid userId, CancellationToken cancellationToken = default);
}
