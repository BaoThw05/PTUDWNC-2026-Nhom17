namespace CulinaryBlog.Application.Features.Auth.Abstractions;

/// <summary>Hợp đồng job nền (FR-JOB-001); được lập lịch qua <c>IBackgroundJobService</c> của TV3 khi bàn giao (việc 1.15).</summary>
public interface IWelcomeEmailJob
{
    Task ExecuteAsync(Guid userId, CancellationToken cancellationToken = default);
}
