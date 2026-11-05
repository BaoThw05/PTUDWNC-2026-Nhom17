using CulinaryBlog.Domain.Auth;

namespace CulinaryBlog.Application.Features.Auth.Abstractions;

public interface ILoginAttemptRepository
{
    /// <summary>Thêm và lưu ngay, để lần đăng nhập thất bại vẫn được ghi dù request kết thúc bằng exception.</summary>
    Task AddAsync(LoginAttempt attempt, CancellationToken cancellationToken);

    /// <summary>Các lần đăng nhập mới nhất trước.</summary>
    Task<IReadOnlyList<LoginAttempt>> ListRecentAsync(Guid userId, int take, CancellationToken cancellationToken);

    Task<int> DeleteBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);
}
