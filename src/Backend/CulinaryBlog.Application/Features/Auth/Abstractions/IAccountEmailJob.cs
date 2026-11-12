namespace CulinaryBlog.Application.Features.Auth.Abstractions;

/// <summary>Job nền gửi email xác nhận địa chỉ và mã đổi email; enqueue qua <c>IBackgroundJobService</c>.</summary>
public interface IAccountEmailJob
{
    Task SendConfirmationAsync(Guid userId, CancellationToken cancellationToken = default);

    Task SendChangeEmailCodeAsync(Guid userId, string newEmail, CancellationToken cancellationToken = default);
}
