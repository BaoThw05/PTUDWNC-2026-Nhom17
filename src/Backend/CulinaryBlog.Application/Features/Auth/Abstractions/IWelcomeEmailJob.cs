namespace CulinaryBlog.Application.Features.Auth.Abstractions;

/// <summary>Hợp đồng job nền (FR-JOB-001); enqueue qua <c>IBackgroundJobService</c> khi đăng ký hoặc tạo tài khoản từ Google (việc 1.15).</summary>
public interface IWelcomeEmailJob
{
    Task ExecuteAsync(Guid userId, CancellationToken cancellationToken = default);
}
